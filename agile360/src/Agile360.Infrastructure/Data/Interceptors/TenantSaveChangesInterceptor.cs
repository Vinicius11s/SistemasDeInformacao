using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Agile360.Infrastructure.Data.Interceptors;

public class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantSaveChangesInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyTenantToNewEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTenantToNewEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantToNewEntities(DbContext? context)
    {
        if (context == null) return;

        var tenantId = _tenantProvider.GetCurrentAdvogadoId();
        if (tenantId == Guid.Empty) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.AdvogadoId = tenantId;
        }
    }
}
