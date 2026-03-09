namespace Agile360.Domain.Entities;

/// <summary>
/// Base para todas as entidades tenant-aware.
/// Colunas reais no banco: id (uuid PK), id_advogado (uuid FK).
/// IsActive e UpdatedAt são mantidos em C# mas ignorados pelo EF
/// nas tabelas que não possuem essas colunas.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Coluna: id</summary>
    public Guid Id { get; set; }

    /// <summary>Coluna: id_advogado</summary>
    public Guid AdvogadoId { get; set; }

    /// <summary>Soft-delete — ignorado nas tabelas sem coluna is_active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>created_at — presente em algumas tabelas (Audiencia, etc.)</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>updated_at — presente em algumas tabelas.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
