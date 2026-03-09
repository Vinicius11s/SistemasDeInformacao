using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

/// <summary>
/// Manages persisted refresh token sessions.
/// All lookups use the SHA-256 hash — raw tokens never touch the DB.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Persist a new session. The session's TokenHash must be a SHA-256 hex string.</summary>
    Task CreateAsync(RefreshTokenSession session, CancellationToken ct = default);

    /// <summary>Find an active (non-revoked, non-expired) session by token hash.</summary>
    Task<RefreshTokenSession?> FindActiveAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Revoke a specific session by token hash (logout single device).</summary>
    Task RevokeAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Revoke all active sessions for an advogado (logout all devices).</summary>
    Task RevokeAllAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>
    /// Hard-deletes all sessions (revoked or expired) where expires_at &lt; now.
    /// Returns the number of rows deleted.
    /// </summary>
    Task<int> DeleteExpiredAsync(CancellationToken ct = default);
}
