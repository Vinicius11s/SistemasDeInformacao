using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class CompromissoRepository : Repository<Compromisso>, ICompromissoRepository
{
    public CompromissoRepository(Agile360DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Compromisso>> GetHojeAsync(CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Data == hoje)
            .OrderBy(c => c.Hora)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Compromisso>> GetSemanaAsync(CancellationToken ct = default)
    {
        var inicio = DateOnly.FromDateTime(DateTime.UtcNow);
        var fim = inicio.AddDays(6);
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Data >= inicio && c.Data <= fim)
            .OrderBy(c => c.Data).ThenBy(c => c.Hora)
            .Take(200) // limita resultado para evitar timeout na leitura do stream (Supabase/remoto)
            .ToListAsync(ct);
    }

    public async Task<Compromisso?> GetProximoAsync(CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Data >= hoje)
            .OrderBy(c => c.Data).ThenBy(c => c.Hora)
            .FirstOrDefaultAsync(ct);
    }
}
