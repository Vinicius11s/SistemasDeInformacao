using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class StagingProcessoRepository : IStagingProcessoRepository
{
    private readonly Agile360DbContext _db;

    public StagingProcessoRepository(Agile360DbContext db) => _db = db;

    public async Task<StagingProcesso> CreateAsync(StagingProcesso item, CancellationToken ct = default)
    {
        _db.StagingProcessos.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public Task<IReadOnlyList<StagingProcesso>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingProcessos
              .Where(s => s.AdvogadoId == advogadoId
                       && s.Status == StagingStatus.Pendente
                       && s.ExpiresAt > DateTimeOffset.UtcNow)
              .OrderByDescending(s => s.CreatedAt)
              .ToListAsync(ct)
              .ContinueWith(t => (IReadOnlyList<StagingProcesso>)t.Result, ct);

    public Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingProcessos.CountAsync(
              s => s.AdvogadoId == advogadoId
                && s.Status == StagingStatus.Pendente
                && s.ExpiresAt > DateTimeOffset.UtcNow, ct);

    public Task<StagingProcesso?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
        => _db.StagingProcessos
              .FirstOrDefaultAsync(s => s.Id == id && s.AdvogadoId == advogadoId, ct);

    public async Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid processoIdGerado, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, advogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente) return false;

        item.Status           = StagingStatus.Confirmado;
        item.ConfirmadoEm     = DateTimeOffset.UtcNow;
        item.ProcessoIdGerado = processoIdGerado;
        item.UpdatedAt        = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejeitarAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, advogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente) return false;

        item.Status      = StagingStatus.Rejeitado;
        item.RejeitadoEm = DateTimeOffset.UtcNow;
        item.UpdatedAt   = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
