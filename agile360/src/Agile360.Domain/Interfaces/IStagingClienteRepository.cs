using Agile360.Domain.Entities;
using Agile360.Domain.Enums;

namespace Agile360.Domain.Interfaces;

/// <summary>
/// Repository for the WhatsApp bot approval queue.
/// All methods enforce AdvogadoId isolation explicitly (no HasQueryFilter here).
/// </summary>
public interface IStagingClienteRepository
{
    Task<StagingCliente> CreateAsync(StagingCliente item, CancellationToken ct = default);

    /// <summary>Lists Pendente records for the given advogado, excluding expired ones.</summary>
    Task<IReadOnlyList<StagingCliente>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>Count of Pendente records — used for the dashboard notification badge.</summary>
    Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>Fetches a single record, ensuring it belongs to the given advogado.</summary>
    Task<StagingCliente?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid clienteIdGerado, CancellationToken ct = default);
    Task<bool> RejeitarAsync(Guid id, Guid advogadoId, CancellationToken ct = default);
}
