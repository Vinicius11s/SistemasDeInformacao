using Agile360.Application.Auth.DTOs;

namespace Agile360.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string accessToken, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<AdvogadoProfileResponse?> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default);

    // ─── MFA ──────────────────────────────────────────────────────────────────

    /// <summary>Valida o token temporário de MFA e retorna o advogadoId, ou null se inválido/expirado.</summary>
    Guid? ValidateMfaTempToken(string tempToken);

    /// <summary>Finaliza o challenge de MFA e emite tokens completos.</summary>
    Task<AuthResult?> CompleteMfaChallengeAsync(string tempToken, string totpCode, CancellationToken cancellationToken = default);
}
