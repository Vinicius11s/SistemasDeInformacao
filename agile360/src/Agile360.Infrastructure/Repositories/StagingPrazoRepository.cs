using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class StagingPrazoRepository : IStagingPrazoRepository
{
    private readonly Agile360DbContext _db;

    public StagingPrazoRepository(Agile360DbContext db) => _db = db;

    public async Task<StagingPrazo> CreateAsync(StagingPrazo item, CancellationToken ct = default)
    {
        _db.StagingPrazos.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public Task<IReadOnlyList<StagingPrazo>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingPrazos
            .Where(s => s.AdvogadoId == advogadoId
                     && s.Status == StagingStatus.Pendente
                     && s.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<StagingPrazo>)t.Result, ct);

    public Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingPrazos.CountAsync(
            s => s.AdvogadoId == advogadoId
              && s.Status == StagingStatus.Pendente
              && s.ExpiresAt > DateTimeOffset.UtcNow, ct);

    public Task<StagingPrazo?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
        => _db.StagingPrazos.FirstOrDefaultAsync(s => s.Id == id && s.AdvogadoId == advogadoId, ct);

    public async Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid prazoIdGerado, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, advogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente) return false;

        item.Status = StagingStatus.Confirmado;
        item.ConfirmadoEm = DateTimeOffset.UtcNow;
        item.PrazoIdGerado = prazoIdGerado;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejeitarAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, advogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente) return false;

        item.Status = StagingStatus.Rejeitado;
        item.RejeitadoEm = DateTimeOffset.UtcNow;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateAsync(StagingPrazo item, CancellationToken ct = default)
    {
        if (item is null) return false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

