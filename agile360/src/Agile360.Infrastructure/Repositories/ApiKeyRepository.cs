using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly Agile360DbContext _db;

    public ApiKeyRepository(Agile360DbContext db) => _db = db;

    public Task<ApiKey?> FindActiveAsync(string keyHash, CancellationToken ct = default)
    {
        return _db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k =>
                k.KeyHash == keyHash &&
                k.RevokedAt == null &&
                (k.ExpiresAt == null || k.ExpiresAt > DateTimeOffset.UtcNow),
                ct);
    }

    public async Task CreateAsync(ApiKey apiKey, CancellationToken ct = default)
    {
        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ApiKey>> ListByAdvogadoAsync(Guid advogadoId, CancellationToken ct = default)
    {
        return await _db.ApiKeys
            .AsNoTracking()
            .Where(k => k.AdvogadoId == advogadoId && k.RevokedAt == null)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> RevokeAsync(Guid id, Guid advogadoId, CancellationToken ct = default)
    {
        var key = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.AdvogadoId == advogadoId && k.RevokedAt == null, ct);
        if (key == null) return false;
        key.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task TouchAsync(Guid id, CancellationToken ct = default)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (key == null) return;
        key.LastUsedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
