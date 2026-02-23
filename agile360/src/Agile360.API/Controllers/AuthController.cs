using Agile360.API.Models;
using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Agile360.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Falha no registro", statusCode: 400));
        return Ok(ApiResponse<AuthResponse>.Ok(result.Data!));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "E-mail ou senha inválidos.", statusCode: 400));
        return Ok(ApiResponse<AuthResponse>.Ok(result.Data!));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        if (result == null || !result.Success)
            return Unauthorized(ApiResponse<object>.Fail("Refresh token inválido ou expirado.", statusCode: 401));
        return Ok(ApiResponse<AuthResponse>.Ok(result.Data!));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = GetBearerToken();
        if (!string.IsNullOrEmpty(token))
            await _authService.LogoutAsync(token, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-forgot")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request.Email, ct);
        return Accepted(ApiResponse<object>.Ok(new { message = "Se o e-mail existir, você receberá um link para redefinir a senha." }));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        try
        {
            await _authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Senha alterada com sucesso." }));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse<object>.Fail("Token inválido ou expirado.", statusCode: 400));
        }
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AdvogadoProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var token = GetBearerToken();
        if (string.IsNullOrEmpty(token))
            return Unauthorized();
        var profile = await _authService.GetProfileAsync(token, ct);
        if (profile == null)
            return Unauthorized();
        return Ok(ApiResponse<AdvogadoProfileResponse>.Ok(profile));
    }

    private string? GetBearerToken()
    {
        var auth = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return auth["Bearer ".Length..].Trim();
    }
}

public record RefreshTokenRequest(string RefreshToken);
