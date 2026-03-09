using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class PrazoRepository : Repository<Prazo>, IPrazoRepository
{
    public PrazoRepository(Agile360DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Prazo>> GetVencimentoProximoAsync(
        int diasAntes, CancellationToken ct = default)
    {
        var hoje   = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoje.AddDays(diasAntes);
        return await _dbSet
            .Where(p => p.Status == "Pendente"
                     && p.DataVencimento >= hoje
                     && p.DataVencimento <= limite)
            .OrderBy(p => p.DataVencimento)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Prazo>> GetPendentesAsync(CancellationToken ct = default) =>
        await _dbSet
            .Where(p => p.Status == "Pendente")
            .OrderBy(p => p.DataVencimento)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Prazo>> GetFataisAsync(CancellationToken ct = default) =>
        await _dbSet
            .Where(p => p.Status == "Pendente" && p.TipoPrazo == "Fatal")
            .OrderBy(p => p.DataVencimento)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Prazo>> GetProximosAsync(int count, CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(p => p.Status == "Pendente" && p.DataVencimento >= hoje)
            .OrderBy(p => p.DataVencimento)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
