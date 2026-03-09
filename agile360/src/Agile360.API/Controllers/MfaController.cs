using Agile360.API.Models;
using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Agile360.API.Controllers;

// B10 — VerifySetup integrado com IRecoveryCodeService:
//   Após CompleteSetupAsync retornar true, os 10 códigos de recuperação são gerados
//   e incluídos na resposta (MfaActivatedResponse). O frontend exibe os códigos no
//   Passo 3 do stepper de segurança — ÚNICA oportunidade de visualização.

/// <summary>
/// Endpoints for TOTP-based MFA (Google Authenticator).
///
/// Setup flow (authenticated advogado):
///   POST   /api/auth/mfa/setup          → generate QR code
///   POST   /api/auth/mfa/verify-setup   → confirm first code → enable MFA
///   DELETE /api/auth/mfa/disable        → disable MFA (requires valid TOTP)
///   GET    /api/auth/mfa/status         → is MFA currently enabled?
///
/// Login challenge (public, rate-limited):
///   POST   /api/auth/mfa/challenge      → validate temp token + TOTP → issue full JWT
/// </summary>
[ApiController]
[Route("api/auth/mfa")]
public class MfaController : ControllerBase
{
    private const string RefreshCookieName = "agile360_rt";
    private static readonly CookieOptions RefreshCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(30),
        Path = "/api/auth",
    };

    private readonly IMfaService _mfa;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly IRecoveryCodeService _recoveryCodes;

    public MfaController(
        IMfaService mfa,
        IAuthService authService,
        ICurrentUserService currentUser,
        IRecoveryCodeService recoveryCodes)
    {
        _mfa           = mfa;
        _authService   = authService;
        _currentUser   = currentUser;
        _recoveryCodes = recoveryCodes;
    }

    // ── GET /api/auth/mfa/status ──────────────────────────────────────────
    // Dashboard: is MFA currently active for the logged-in advogado?

    [HttpGet("status")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MfaStatusResponse>), 200)]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var status = await _mfa.GetStatusAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<MfaStatusResponse>.Ok(status));
    }

    // ── POST /api/auth/mfa/setup ──────────────────────────────────────────
    // Step 1 of setup: generate a pending secret and return the QR code URL.

    [HttpPost("setup")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MfaSetupResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<IActionResult> Setup(CancellationToken ct)
    {
        try
        {
            var response = await _mfa.BeginSetupAsync(_currentUser.AdvogadoId, _currentUser.Email, ct);
            return Ok(ApiResponse<MfaSetupResponse>.Ok(response));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("MFA") || ex.Message.Contains("EncryptionKey"))
        {
            // MfaSettings:EncryptionKey não configurado → DisabledMfaService ativo.
            // Retorna 503 com mensagem amigável em vez de 500.
            return StatusCode(503, ApiResponse<object>.Fail(
                "O serviço de autenticação de dois fatores não está habilitado neste servidor. " +
                "Configure MfaSettings:EncryptionKey nos segredos da aplicação."));
        }
    }

    // ── POST /api/auth/mfa/verify-setup ──────────────────────────────────
    // Step 2 of setup: confirm the first TOTP code — activates MFA.
    // B10: Após ativar, gera os 10 recovery codes e os inclui na resposta.
    // Frontend exibe os códigos no Passo 3 — ÚNICA oportunidade de visualização.

    [HttpPost("verify-setup")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MfaActivatedResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> VerifySetup([FromBody] MfaVerifySetupRequest request, CancellationToken ct)
    {
        var ok = await _mfa.CompleteSetupAsync(_currentUser.AdvogadoId, request.Code, ct);
        if (!ok)
            return BadRequest(ApiResponse<object>.Fail(
                "Código inválido ou sessão de setup expirada. Reinicie o processo.", statusCode: 400));

        // B10: Gera os 10 códigos de recuperação e retorna o plaintext na resposta.
        // GenerateCodesAsync invalida quaisquer códigos anteriores antes de criar novos.
        var codes = await _recoveryCodes.GenerateCodesAsync(_currentUser.AdvogadoId, ct);

        return Ok(ApiResponse<MfaActivatedResponse>.Ok(new MfaActivatedResponse(true, codes)));
    }

    // ── DELETE /api/auth/mfa/disable ──────────────────────────────────────
    // Disables MFA after validating the current TOTP code.

    [HttpDelete("disable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MfaStatusResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Disable([FromBody] MfaDisableRequest request, CancellationToken ct)
    {
        var ok = await _mfa.DisableAsync(_currentUser.AdvogadoId, request.Code, ct);
        if (!ok)
            return BadRequest(ApiResponse<object>.Fail(
                "Código inválido. Confirme o código do Google Authenticator.", statusCode: 400));

        // B11: Ao desativar o MFA, deleta todos os recovery codes do advogado (hard delete).
        // Evita que códigos orfãos consumam espaço e não possam mais ser usados para login.
        await _recoveryCodes.DeleteAllAsync(_currentUser.AdvogadoId, ct);

        return Ok(ApiResponse<MfaStatusResponse>.Ok(new MfaStatusResponse(false)));
    }

    // ── POST /api/auth/mfa/challenge ──────────────────────────────────────
    // Public endpoint: validates temp token + TOTP code, issues full JWT.
    // Called by the frontend on the MFA input page after the first login step.

    [HttpPost("challenge")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(ApiResponse<SecureAuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Challenge([FromBody] MfaChallengeRequest request, CancellationToken ct)
    {
        var advogadoId = _authService.ValidateMfaTempToken(request.MfaTempToken);
        if (advogadoId == null)
            return Unauthorized(ApiResponse<object>.Fail(
                "Token expirado ou inválido. Faça login novamente.", statusCode: 401));

        // Validate TOTP code
        var codeOk = await _mfa.ValidateCodeAsync(advogadoId.Value, request.Code, ct);
        if (!codeOk)
            return Unauthorized(ApiResponse<object>.Fail(
                "Código inválido. Verifique o Google Authenticator.", statusCode: 401));

        // Issue full tokens
        var result = await _authService.CompleteMfaChallengeAsync(request.MfaTempToken, request.Code, ct);
        if (result == null || !result.Success)
            return Unauthorized(ApiResponse<object>.Fail("Falha na autenticação.", statusCode: 401));

        return Ok(BuildSecureResponse(result.Data!));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private ApiResponse<SecureAuthResponse> BuildSecureResponse(AuthResponse data)
    {
        Response.Cookies.Append(RefreshCookieName, data.RefreshToken, RefreshCookieOptions);
        var secure = new SecureAuthResponse(
            AccessToken: data.AccessToken,
            ExpiresInSeconds: 15 * 60,
            Advogado: data.Advogado);
        return ApiResponse<SecureAuthResponse>.Ok(secure);
    }
}
