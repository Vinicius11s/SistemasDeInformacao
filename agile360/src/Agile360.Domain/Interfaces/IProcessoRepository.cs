using Agile360.Domain.Entities;
using Agile360.Domain.Enums;

namespace Agile360.Domain.Interfaces;

public interface IProcessoRepository : IRepository<Processo>
{
    Task<Processo?> GetByNumeroAsync(string numeroProcesso, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Processo>> GetRecentesAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Processo>> GetByStatusAsync(StatusProcesso status, CancellationToken cancellationToken = default);
}
