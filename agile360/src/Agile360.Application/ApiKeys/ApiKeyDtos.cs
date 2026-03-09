namespace Agile360.Application.ApiKeys;

/// <summary>
/// Returned ONCE at creation. RawKey is never stored — advogado must copy it immediately.
/// </summary>
public record CreateApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    string RawKey,          // shown once — never again
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt);

public record ApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? ExpiresAt);

public record CreateApiKeyRequest(
    string Name,
    DateTimeOffset? ExpiresAt);
