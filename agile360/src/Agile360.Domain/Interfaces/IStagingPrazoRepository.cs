using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

/// <summary>
/// Repositório da fila de aprovação de prazos via WhatsApp/n8n.
/// </summary>
public interface IStagingPrazoRepository
{
    Task<StagingPrazo> CreateAsync(StagingPrazo item, CancellationToken ct = default);
    Task<IReadOnlyList<StagingPrazo>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default);
    Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default);
    Task<StagingPrazo?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid prazoIdGerado, CancellationToken ct = default);
    Task<bool> RejeitarAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    Task<bool> UpdateAsync(StagingPrazo item, CancellationToken ct = default);
}

