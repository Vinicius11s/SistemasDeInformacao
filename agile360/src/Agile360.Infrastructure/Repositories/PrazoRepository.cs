using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class PrazoRepository : Repository<Prazo>, IPrazoRepository
{
    public PrazoRepository(Agile360DbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Prazo>> GetVencimentoProximoAsync(int horasAntes, CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var limite = agora.AddHours(horasAntes);
        return await _dbSet
            .Where(p => p.Status == StatusPrazo.Pendente && p.DataVencimento >= agora && p.DataVencimento <= limite)
            .OrderBy(p => p.DataVencimento)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prazo>> GetPendentesAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.Where(p => p.Status == StatusPrazo.Pendente).OrderBy(p => p.DataVencimento).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Prazo>> GetFataisAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .Where(p => p.Status == StatusPrazo.Pendente && p.Tipo == TipoPrazo.Fatal)
            .OrderBy(p => p.DataVencimento)
            .ToListAsync(cancellationToken);
}
