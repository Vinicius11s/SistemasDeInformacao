using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface IPrazoRepository : IRepository<Prazo>
{
    Task<IReadOnlyList<Prazo>> GetVencimentoProximoAsync(int horasAntes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prazo>> GetPendentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prazo>> GetFataisAsync(CancellationToken cancellationToken = default);
}
