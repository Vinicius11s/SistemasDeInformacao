using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class StagingClienteRepository : IStagingClienteRepository
{
    private readonly Agile360DbContext _db;

    public StagingClienteRepository(Agile360DbContext db) => _db = db;

    public async Task<StagingCliente> CreateAsync(StagingCliente item, CancellationToken ct = default)
    {
        _db.StagingClientes.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public Task<IReadOnlyList<StagingCliente>> ListPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingClientes
              .Where(s => s.AdvogadoId == advogadoId
                       && s.Status == StagingStatus.Pendente
                       && s.ExpiresAt > DateTimeOffset.UtcNow)
              .OrderByDescending(s => s.CreatedAt)
              .ToListAsync(ct)
              .ContinueWith(t => (IReadOnlyList<StagingCliente>)t.Result, ct);

    public Task<int> CountPendentesAsync(Guid advogadoId, CancellationToken ct = default)
        => _db.StagingClientes.CountAsync(
              s => s.AdvogadoId == advogadoId
                && s.Status == StagingStatus.Pendente
                && s.ExpiresAt > DateTimeOffset.UtcNow, ct);

    public Task<StagingCliente?> GetByIdAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
        => _db.StagingClientes
              .FirstOrDefaultAsync(s => s.Id == id && s.AdvogadoId == advogadoId, ct);

    public async Task<bool> ConfirmarAsync(Guid id, Guid advogadoId, Guid clienteIdGerado, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, advogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente) return false;

        item.Status = StagingStatus.Confirmado;
        item.ConfirmadoEm = DateTimeOffset.UtcNow;
        item.ClienteIdGerado = clienteIdGerado;
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
}
