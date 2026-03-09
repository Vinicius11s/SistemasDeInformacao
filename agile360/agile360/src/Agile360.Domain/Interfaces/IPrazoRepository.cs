using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface IPrazoRepository : IRepository<Prazo>
{
    /// <summary>
    /// Retorna prazos com status=Pendente e data_vencimento entre hoje e hoje+diasAntes.
    /// Usado para alertas de vencimento próximo.
    /// </summary>
    Task<IReadOnlyList<Prazo>> GetVencimentoProximoAsync(int diasAntes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os prazos com status=Pendente, ordenados por data_vencimento ASC.
    /// </summary>
    Task<IReadOnlyList<Prazo>> GetPendentesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna prazos com prioridade=Fatal e status=Pendente.
    /// </summary>
    Task<IReadOnlyList<Prazo>> GetFataisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna os próximos <paramref name="count"/> prazos pendentes a partir de hoje.
    /// Usado pelo Dashboard (card "Próximos Prazos").
    /// </summary>
    Task<IReadOnlyList<Prazo>> GetProximosAsync(int count, CancellationToken cancellationToken = default);
}
