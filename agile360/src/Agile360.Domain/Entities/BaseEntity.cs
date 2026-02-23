namespace Agile360.Domain.Entities;

/// <summary>
/// Base entity for all tenant-aware entities. Id, audit fields, and AdvogadoId (tenant key).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid AdvogadoId { get; set; }
}
