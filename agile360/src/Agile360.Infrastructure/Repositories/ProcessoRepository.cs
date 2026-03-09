using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class ProcessoRepository : Repository<Processo>, IProcessoRepository
{
    public ProcessoRepository(Agile360DbContext context) : base(context) { }

    public async Task<Processo?> GetByNumeroAsync(string numeroProcesso, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.NumProcesso == numeroProcesso, ct);

    public async Task<IReadOnlyList<Processo>> GetRecentesAsync(int count, CancellationToken ct = default) =>
        await _dbSet
            .OrderByDescending(p => p.CriadoEm)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Processo>> GetByStatusAsync(
        Agile360.Domain.Enums.StatusProcesso status, CancellationToken ct = default) =>
        await _dbSet
            .Where(p => p.Status == status.ToString())
            .AsNoTracking()
            .ToListAsync(ct);
}
