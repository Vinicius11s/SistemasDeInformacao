using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

/// <summary>
/// Repositório da fila de aprovação de compromissos via WhatsApp/n8n.
/// Todos os métodos aplicam isolamento por AdvogadoId explicitamente
/// (sem HasQueryFilter — staging não herda BaseEntity).
/// </summary>
public interface IStagingCompromissoRepository
{
    Task<StagingCompromisso> CreateAsync(StagingCompromisso item, CancellationToken ct = default);

    /// <summary>Lista registros Pendente do advogado, excluindo expirados.</summary>
    Task<IReadOnlyList<StagingCompromisso>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>Contagem de Pendentes — usado para o badge de notificação.</summary>
    Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>Busca um registro garantindo que pertence ao advogado.</summary>
    Task<StagingCompromisso?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid compromissoIdGerado, CancellationToken ct = default);
    Task<bool> RejeitarAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    /// <summary>Persiste alterações do staging e atualiza UpdatedAt.</summary>
    Task<bool> UpdateAsync(StagingCompromisso item, CancellationToken ct = default);
}
