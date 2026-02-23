using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class AudienciaRepository : Repository<Audiencia>, IAudienciaRepository
{
    public AudienciaRepository(Agile360DbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Audiencia>> GetHojeAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateTimeOffset.UtcNow.Date;
        var fimDoDia = hoje.AddDays(1);
        return await _dbSet
            .Where(a => a.DataHora >= hoje && a.DataHora < fimDoDia)
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audiencia>> GetSemanaAsync(CancellationToken cancellationToken = default)
    {
        var inicio = DateTimeOffset.UtcNow.Date;
        var fim = inicio.AddDays(7);
        return await _dbSet
            .Where(a => a.DataHora >= inicio && a.DataHora < fim)
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<Audiencia?> GetProximaAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .Where(a => a.DataHora >= DateTimeOffset.UtcNow)
            .OrderBy(a => a.DataHora)
            .FirstOrDefaultAsync(cancellationToken);
}
