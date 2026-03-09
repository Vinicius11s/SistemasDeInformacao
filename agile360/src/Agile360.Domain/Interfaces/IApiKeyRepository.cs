using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface IApiKeyRepository
{
    /// <summary>Find an active key by its SHA-256 hash. Returns null if not found or revoked/expired.</summary>
    Task<ApiKey?> FindActiveAsync(string keyHash, CancellationToken ct = default);

    /// <summary>Persist a new key. KeyHash must already be a SHA-256 hex string.</summary>
    Task CreateAsync(ApiKey apiKey, CancellationToken ct = default);

    /// <summary>List all non-revoked keys belonging to an advogado.</summary>
    Task<IReadOnlyList<ApiKey>> ListByAdvogadoAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>Revoke a specific key by its ID (only if it belongs to the given advogado).</summary>
    Task<bool> RevokeAsync(Guid id, Guid advogadoId, CancellationToken ct = default);

    /// <summary>Update LastUsedAt timestamp (fire-and-forget friendly).</summary>
    Task TouchAsync(Guid id, CancellationToken ct = default);
}
