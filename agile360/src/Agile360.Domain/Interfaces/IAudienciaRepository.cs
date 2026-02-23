using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface IAudienciaRepository : IRepository<Audiencia>
{
    Task<IReadOnlyList<Audiencia>> GetHojeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audiencia>> GetSemanaAsync(CancellationToken cancellationToken = default);
    Task<Audiencia?> GetProximaAsync(CancellationToken cancellationToken = default);
}
