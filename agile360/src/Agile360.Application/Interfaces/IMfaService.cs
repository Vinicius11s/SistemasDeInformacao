using Agile360.Application.Auth.DTOs;

namespace Agile360.Application.Interfaces;

/// <summary>
/// TOTP-based MFA service.
/// Encryption of the secret is handled internally — callers never touch plaintext TOTP keys.
/// </summary>
public interface IMfaService
{
    // ── Setup flow ──────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a fresh TOTP secret, persists it as the pending secret (encrypted),
    /// and returns the otpauth:// URL for QR code display.
    /// </summary>
    Task<MfaSetupResponse> BeginSetupAsync(Guid advogadoId, string email, CancellationToken ct = default);

    /// <summary>
    /// Verifies the first TOTP code against the pending secret.
    /// On success, promotes the pending secret → active secret and sets MfaEnabled = true.
    /// </summary>
    Task<bool> CompleteSetupAsync(Guid advogadoId, string code, CancellationToken ct = default);

    /// <summary>
    /// Validates the current TOTP code then clears mfa_secret and sets MfaEnabled = false.
    /// </summary>
    Task<bool> DisableAsync(Guid advogadoId, string code, CancellationToken ct = default);

    // ── Login flow ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a TOTP code against the active (encrypted) mfa_secret of the given advogado.
    /// </summary>
    Task<bool> ValidateCodeAsync(Guid advogadoId, string code, CancellationToken ct = default);

    // ── Status ──────────────────────────────────────────────────────────────

    Task<MfaStatusResponse> GetStatusAsync(Guid advogadoId, CancellationToken ct = default);
}
