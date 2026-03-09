using System.Collections.Generic;
using System.Text.Json;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Agile360.Infrastructure.Data.Interceptors;

/// <summary>
/// Story 1.2.1: Sets shadow properties CreatedBy/LastModifiedBy and writes to audit_logs for Processo/Prazo.
/// When no user context (e.g. webhook), CreatedBy/LastModifiedBy remain null.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditableEntityNames = new(StringComparer.Ordinal)
    {
        nameof(Processo), nameof(Prazo), nameof(Cliente), nameof(Compromisso), nameof(Nota)
    };

    private static readonly HashSet<string> AuditLogEntityNames = new(StringComparer.Ordinal)
    {
        nameof(Processo), nameof(Prazo)
    };

    private const string CreatedByShadow = "CreatedBy";
    private const string LastModifiedByShadow = "LastModifiedBy";

    private readonly ITenantProvider _tenantProvider;

    public AuditSaveChangesInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null) return;

        var currentUserId = _tenantProvider.GetCurrentAdvogadoId();
        var userIdNullable = currentUserId == Guid.Empty ? (Guid?)null : currentUserId;
        var now = DateTimeOffset.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity baseEntity) continue;
            var entityName = entry.Metadata.ClrType.Name;
            if (!AuditableEntityNames.Contains(entityName)) continue;

            if (entry.State == EntityState.Added)
            {
                SetShadow(entry, CreatedByShadow, userIdNullable);
                SetShadow(entry, LastModifiedByShadow, userIdNullable);
                baseEntity.UpdatedAt = now;
                if (AuditLogEntityNames.Contains(entityName))
                    auditLogs.Add(CreateAuditLog(context, entityName, baseEntity.Id, AuditAction.Created, null, GetCurrentValues(entry), currentUserId));
            }
            else if (entry.State == EntityState.Modified)
            {
                SetShadow(entry, LastModifiedByShadow, userIdNullable);
                baseEntity.UpdatedAt = now;
                if (AuditLogEntityNames.Contains(entityName))
                    auditLogs.Add(CreateAuditLog(context, entityName, baseEntity.Id, AuditAction.Updated, GetOriginalValues(entry), GetCurrentValues(entry), currentUserId));
            }
            else if (entry.State == EntityState.Deleted && AuditLogEntityNames.Contains(entityName))
            {
                auditLogs.Add(CreateAuditLog(context, entityName, baseEntity.Id, AuditAction.Deleted, GetOriginalValues(entry), null, currentUserId));
            }
        }

        foreach (var log in auditLogs)
            context.Set<AuditLog>().Add(log);
    }

    private static void SetShadow(EntityEntry entry, string propertyName, Guid? value)
    {
        var prop = entry.Property(propertyName);
        if (prop != null)
            prop.CurrentValue = value;
    }

    private static AuditLog CreateAuditLog(DbContext context, string entityName, Guid entityId, AuditAction action, string? oldValues, string? newValues, Guid advogadoId)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            AdvogadoId = advogadoId == Guid.Empty ? null : advogadoId,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedAt = DateTimeOffset.UtcNow
        };
    }

    private static string? GetOriginalValues(EntityEntry entry)
    {
        return GetValuesJson(entry, useOriginal: true);
    }

    private static string? GetCurrentValues(EntityEntry entry)
    {
        return GetValuesJson(entry, useOriginal: false);
    }

    private static string? GetValuesJson(EntityEntry entry, bool useOriginal)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.Name is CreatedByShadow or LastModifiedByShadow)
                continue;
            var value = useOriginal ? prop.OriginalValue : prop.CurrentValue;
            if (value != null && (value is DateTime or DateTimeOffset))
                dict[prop.Metadata.Name] = value is DateTimeOffset dto ? dto.UtcDateTime : value;
            else
                dict[prop.Metadata.Name] = value;
        }
        return dict.Count == 0 ? null : JsonSerializer.Serialize(dict);
    }
}
