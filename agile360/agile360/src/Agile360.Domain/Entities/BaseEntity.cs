namespace Agile360.Domain.Entities;

/// <summary>
/// Base para todas as entidades tenant-aware.
/// id_advogado é a FK para auth.users (chave de tenant/RLS).
/// Timestamps são gerenciados pelo Supabase (DEFAULT now()).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>id — uuid (PK)</summary>
    public Guid Id { get; set; }

    /// <summary>id_advogado — uuid (FK para auth.users / tenant key)</summary>
    public Guid IdAdvogado { get; set; }
}
