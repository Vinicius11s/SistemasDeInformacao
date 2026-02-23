using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class ProcessoRepository : Repository<Processo>, IProcessoRepository
{
    public ProcessoRepository(Agile360DbContext context) : base(context)
    {
    }

    public async Task<Processo?> GetByNumeroAsync(string numeroProcesso, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.NumeroProcesso == numeroProcesso, cancellationToken);

    public async Task<IReadOnlyList<Processo>> GetRecentesAsync(int count, CancellationToken cancellationToken = default) =>
        await _dbSet.OrderByDescending(p => p.UpdatedAt).Take(count).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Processo>> GetByStatusAsync(StatusProcesso status, CancellationToken cancellationToken = default) =>
        await _dbSet.Where(p => p.Status == status).ToListAsync(cancellationToken);
}
