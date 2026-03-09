namespace Agile360.Application.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    AdvogadoProfileResponse Advogado);

/// <summary>
/// Response seguro — refresh token vai em cookie HttpOnly, não no body.
/// Evita exposição do refresh token via JavaScript.
/// </summary>
public record SecureAuthResponse(
    string AccessToken,
    int ExpiresInSeconds,
    AdvogadoProfileResponse Advogado);

public record AuthResult(bool Success, AuthResponse? Data, string? Error)
{
    public static AuthResult Ok(AuthResponse data) => new(true, data, null);
    public static AuthResult Fail(string error) => new(false, null, error);
}
