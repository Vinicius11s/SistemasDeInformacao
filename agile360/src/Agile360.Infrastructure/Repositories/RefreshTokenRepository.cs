using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly Agile360DbContext _db;

    public RefreshTokenRepository(Agile360DbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(RefreshTokenSession session, CancellationToken ct = default)
    {
        _db.RefreshTokenSessions.Add(session);
        await _db.SaveChangesAsync(ct);
    }

    public Task<RefreshTokenSession?> FindActiveAsync(string tokenHash, CancellationToken ct = default)
    {
        return _db.RefreshTokenSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.TokenHash == tokenHash && s.RevokedAt == null && s.ExpiresAt > DateTimeOffset.UtcNow,
                ct);
    }

    public async Task RevokeAsync(string tokenHash, CancellationToken ct = default)
    {
        var session = await _db.RefreshTokenSessions
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash && s.RevokedAt == null, ct);
        if (session == null) return;
        session.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(Guid advogadoId, CancellationToken ct = default)
    {
        var sessions = await _db.RefreshTokenSessions
            .Where(s => s.AdvogadoId == advogadoId && s.RevokedAt == null)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var s in sessions) s.RevokedAt = now;
        await _db.SaveChangesAsync(ct);
    }

    public Task<int> DeleteExpiredAsync(CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow;
        // Uses idx_rts_expires_at partial index for an efficient bulk delete
        return _db.RefreshTokenSessions
            .Where(s => s.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
