namespace Agile360.Application.Auth.DTOs;

// ── Setup ──────────────────────────────────────────────────────────────────

/// <summary>Returned by POST /api/auth/mfa/setup — shows QR code to the user.</summary>
public record MfaSetupResponse(
    string QrCodeUrl,
    string ManualEntryKey,
    bool MfaEnabled);

/// <summary>Request to verify the first TOTP code and activate MFA.</summary>
public record MfaVerifySetupRequest(string Code);

/// <summary>Request to disable MFA (requires current valid TOTP code for safety).</summary>
public record MfaDisableRequest(string Code);

// ── Login challenge ─────────────────────────────────────────────────────────

/// <summary>
/// Returned instead of a full SecureAuthResponse when the advogado has MFA enabled.
/// The frontend redirects to the TOTP input page with this token.
/// </summary>
public record MfaRequiredResponse(
    string MfaTempToken,
    int ExpiresInSeconds = 300);   // 5 minutes

/// <summary>Request body sent to POST /api/auth/mfa/challenge.</summary>
public record MfaChallengeRequest(string MfaTempToken, string Code);

// ── Status ──────────────────────────────────────────────────────────────────

/// <summary>Lightweight status for the profile / security settings page.</summary>
public record MfaStatusResponse(bool MfaEnabled);

// ── Ativação + Recovery Codes ────────────────────────────────────────────────

/// <summary>
/// Resposta do POST /api/auth/mfa/verify-setup.
/// Inclui o status de ativação E os 10 códigos de recuperação em plaintext.
/// Os códigos são exibidos APENAS nesta resposta — única oportunidade de visualização.
/// </summary>
public record MfaActivatedResponse(
    bool MfaEnabled,
    IReadOnlyList<string> RecoveryCodes);

// ── Auth extension ─────────────────────────────────────────────────────────

/// <summary>
/// Extended login result that can carry either the full tokens or an MFA challenge.
/// </summary>
public record LoginResult(
    bool Success,
    bool MfaRequired,
    AuthResponse? Data,          // full tokens — populated when MfaRequired = false
    MfaRequiredResponse? Mfa,    // temp token  — populated when MfaRequired = true
    string? Error)
{
    public static LoginResult Ok(AuthResponse data) =>
        new(true, false, data, null, null);

    public static LoginResult MfaChallenge(MfaRequiredResponse mfa) =>
        new(true, true, null, mfa, null);

    public static LoginResult Fail(string error) =>
        new(false, false, null, null, error);
}
