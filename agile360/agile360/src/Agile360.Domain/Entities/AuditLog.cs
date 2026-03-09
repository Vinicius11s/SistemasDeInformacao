using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Audit log entry for entity changes (Story 1.2.1). Scoped by AdvogadoId; RLS enforces tenant isolation.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public AuditAction Action { get; set; }
    public Guid? AdvogadoId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? IpAddress { get; set; }
}
