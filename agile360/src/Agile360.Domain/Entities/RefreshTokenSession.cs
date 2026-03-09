namespace Agile360.Domain.Entities;

/// <summary>
/// Persisted refresh token session. Only the SHA-256 hash of the raw token is stored.
/// </summary>
public class RefreshTokenSession
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    /// <summary>SHA-256 hex of the raw refresh token. Never store the raw value.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>NULL = active; NOT NULL = revoked.</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTimeOffset.UtcNow;
}
