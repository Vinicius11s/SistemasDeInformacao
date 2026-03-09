using System.Security.Cryptography;
using Agile360.API.Models;
using Agile360.Application.ApiKeys;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.API.Auth;
using Agile360.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Manages long-lived API keys for M2M integrations (n8n, WhatsApp bots, etc.).
/// All endpoints require a valid JWT — only a logged-in advogado can manage their keys.
/// </summary>
[ApiController]
[Route("api/api-keys")]
[Authorize]   // JWT required to create/list/revoke keys
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public ApiKeyController(IApiKeyRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    // ── POST /api/api-keys ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a new API key. The raw key is returned ONCE in this response.
    /// The advogado must copy it immediately — it cannot be retrieved again.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse<object>.Fail("O nome da chave é obrigatório.", statusCode: 400));

        // Generate a cryptographically random key with a readable prefix
        var rawBytes  = RandomNumberGenerator.GetBytes(32);
        var rawSuffix = Convert.ToBase64String(rawBytes)
            .Replace("+", "").Replace("/", "").Replace("=", "")
            .ToLowerInvariant();
        var rawKey    = $"a360_{rawSuffix[..32]}"; // e.g.: a360_x7kqz...
        var keyPrefix = rawKey[..12];               // e.g.: a360_x7kqz0

        var apiKey = new ApiKey
        {
            Id          = Guid.NewGuid(),
            AdvogadoId  = _currentUser.AdvogadoId,
            Name        = request.Name.Trim(),
            KeyHash     = TokenHasher.Hash(rawKey),
            KeyPrefix   = keyPrefix,
            CreatedAt   = DateTimeOffset.UtcNow,
            ExpiresAt   = request.ExpiresAt,
        };

        await _repo.CreateAsync(apiKey, ct);

        var response = new CreateApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            apiKey.KeyPrefix,
            rawKey,          // ← raw key shown ONCE here
            apiKey.CreatedAt,
            apiKey.ExpiresAt);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CreateApiKeyResponse>.Ok(response));
    }

    // ── GET /api/api-keys ────────────────────────────────────────────────────

    /// <summary>Lists all active API keys for the authenticated advogado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApiKeyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var keys = await _repo.ListByAdvogadoAsync(_currentUser.AdvogadoId, ct);
        var dtos  = keys.Select(k => new ApiKeyResponse(
            k.Id, k.Name, k.KeyPrefix, k.CreatedAt, k.LastUsedAt, k.ExpiresAt))
            .ToList();
        return Ok(ApiResponse<IReadOnlyList<ApiKeyResponse>>.Ok(dtos));
    }

    // ── DELETE /api/api-keys/{id} ────────────────────────────────────────────

    /// <summary>Revokes (soft-deletes) an API key. Immediate effect — in-flight requests using this key will fail.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var revoked = await _repo.RevokeAsync(id, _currentUser.AdvogadoId, ct);
        if (!revoked)
            return NotFound(ApiResponse<object>.Fail("Chave não encontrada.", statusCode: 404));
        return NoContent();
    }
}
