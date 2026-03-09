using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface ICompromissoRepository : IRepository<Compromisso>
{
    Task<IReadOnlyList<Compromisso>> GetHojeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Compromisso>> GetSemanaAsync(CancellationToken cancellationToken = default);
    Task<Compromisso?> GetProximoAsync(CancellationToken cancellationToken = default);
}
