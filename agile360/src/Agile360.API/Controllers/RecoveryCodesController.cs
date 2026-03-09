using Agile360.API.Models;
using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Agile360.API.Controllers;

/// <summary>
/// Endpoints para Códigos de Recuperação de Emergência (MFA Backup Codes).
///
/// Roteamento:
///   POST   /api/auth/mfa/recovery-codes/generate   → Gera (ou regenera) 10 códigos — JWT + rate limit
///   GET    /api/auth/mfa/recovery-codes/count       → Retorna quantidade de códigos não usados
///   POST   /api/auth/mfa/challenge/recovery         → Login com código de recuperação (anônimo + rate limit)
///
/// Políticas de segurança:
///   - /generate: requer MfaEnabled = true; limitado a 3 req/hora para proteger CPU (10 × BCrypt).
///   - /challenge/recovery: herda rate limit "auth-login" (5 req/min) — mesmo nível do TOTP.
///   - Todos os erros retornam mensagem genérica para evitar enumeração de códigos válidos.
/// </summary>
[ApiController]
[Route("api/auth/mfa")]
public class RecoveryCodesController : ControllerBase
{
    private readonly IRecoveryCodeService _recoveryCodes;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;
    private readonly IMfaService _mfa;

    // Mensagem genérica — não diferencia "código inválido" de "código inexistente"
    private const string InvalidCodeMessage = "Código de recuperação inválido ou já utilizado.";

    public RecoveryCodesController(
        IRecoveryCodeService recoveryCodes,
        ICurrentUserService currentUser,
        IAuthService authService,
        IMfaService mfa)
    {
        _recoveryCodes = recoveryCodes;
        _currentUser   = currentUser;
        _authService   = authService;
        _mfa           = mfa;
    }

    // ── POST /api/auth/mfa/recovery-codes/generate ────────────────────────
    // Gera 10 novos códigos (invalida os anteriores). Requer MFA ativo.
    // Rate limit: 3 req/hora — geração envolve 10 × BCrypt(cost 12) ≈ 2.5s de CPU.

    /// <summary>
    /// Gera (ou regenera) os 10 códigos de recuperação de emergência.
    /// Os códigos em plaintext são retornados APENAS nesta resposta — nunca mais exibidos.
    /// Requer que o MFA (TOTP) esteja ativo para o advogado.
    /// </summary>
    [HttpPost("recovery-codes/generate")]
    [Authorize]
    [EnableRateLimiting("mfa-generate")]
    [ProducesResponseType(typeof(ApiResponse<RecoveryCodesGenerateResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        // Guard: só permite geração se o MFA já está ativo — evita gerar backup
        // para uma conta sem 2FA habilitado.
        var advogadoId = _currentUser.AdvogadoId;
        var mfaStatus = await _mfa.GetStatusAsync(advogadoId, ct);
        if (!mfaStatus.MfaEnabled)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Os códigos de recuperação só podem ser gerados com a autenticação em dois fatores ativa. " +
                "Ative o 2FA primeiro nas configurações de segurança.",
                statusCode: 400));
        }

        var plainCodes = await _recoveryCodes.GenerateCodesAsync(advogadoId, ct);

        return Ok(ApiResponse<RecoveryCodesGenerateResponse>.Ok(
            new RecoveryCodesGenerateResponse(plainCodes)));
    }

    // ── GET /api/auth/mfa/recovery-codes/count ────────────────────────────
    // Retorna quantos códigos não usados o advogado ainda possui.
    // Sem rate limit — operação de leitura leve, usada para exibir badge na UI.

    /// <summary>
    /// Retorna a contagem de códigos de recuperação ainda disponíveis (não utilizados).
    /// </summary>
    [HttpGet("recovery-codes/count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RecoveryCodesCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var remaining = await _recoveryCodes.GetRemainingCountAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<RecoveryCodesCountResponse>.Ok(
            new RecoveryCodesCountResponse(remaining)));
    }

    // ── POST /api/auth/mfa/challenge/recovery ─────────────────────────────
    // Login com código de recuperação — alternativa ao TOTP.
    // Anônimo + rate limit herdado de "auth-login" (5 req/min).

    /// <summary>
    /// Valida um código de recuperação de emergência no fluxo de login MFA.
    /// Burn-after-use: o código é invalidado imediatamente após uso bem-sucedido.
    /// Retorna mensagem genérica em caso de falha para evitar timing/enumeration attacks.
    /// </summary>
    [HttpPost("challenge/recovery")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(ApiResponse<SecureAuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ChallengeWithRecovery(
        [FromBody] RecoveryChallengeRequest request, CancellationToken ct)
    {
        // 1. Valida o temp token do fluxo MFA
        var advogadoId = _authService.ValidateMfaTempToken(request.MfaTempToken);
        if (advogadoId is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                "Token expirado ou inválido. Faça login novamente.", statusCode: 401));
        }

        // 2. Valida e consome o código de recuperação (atômico — B7a)
        var consumed = await _recoveryCodes.ValidateAndConsumeAsync(advogadoId.Value, request.Code, ct);
        if (!consumed)
        {
            // Mensagem genérica: não diferencia "inválido", "já usado" ou "inexistente"
            return Unauthorized(ApiResponse<object>.Fail(InvalidCodeMessage, statusCode: 401));
        }

        // 3. Emite tokens JWT completos via AuthService (mesmo fluxo do TOTP)
        var result = await _authService.CompleteMfaChallengeAsync(request.MfaTempToken, request.Code, ct);
        if (result is null || !result.Success)
        {
            return Unauthorized(ApiResponse<object>.Fail("Falha na autenticação.", statusCode: 401));
        }

        // 4. Retorna access token; refresh token no cookie HttpOnly (mesmo padrão do MfaController)
        Response.Cookies.Append(RefreshCookieName, result.Data!.RefreshToken, RefreshCookieOptions);
        var secure = new SecureAuthResponse(
            AccessToken:      result.Data.AccessToken,
            ExpiresInSeconds: 15 * 60,
            Advogado:         result.Data.Advogado);

        return Ok(ApiResponse<SecureAuthResponse>.Ok(secure));
    }

    // ── Cookie config (mesma política do MfaController) ──────────────────

    private const string RefreshCookieName = "agile360_rt";
    private static readonly CookieOptions RefreshCookieOptions = new()
    {
        HttpOnly = true,
        Secure   = true,
        SameSite = SameSiteMode.Strict,
        MaxAge   = TimeSpan.FromDays(30),
        Path     = "/api/auth",
    };
}

// ── DTOs locais (sem MediatR — mantém padrão do projeto) ─────────────────────

/// <summary>Resposta da geração de códigos — os plaintext são exibidos APENAS aqui.</summary>
public record RecoveryCodesGenerateResponse(IReadOnlyList<string> Codes);

/// <summary>Resposta da consulta de quantidade de códigos restantes.</summary>
public record RecoveryCodesCountResponse(int Remaining);

/// <summary>Request do login com código de recuperação de emergência.</summary>
public record RecoveryChallengeRequest(string MfaTempToken, string Code);
