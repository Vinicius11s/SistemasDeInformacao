using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Agile360.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly SupabaseAuthClient _authClient;
    private readonly Agile360DbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(SupabaseAuthClient authClient, Agile360DbContext db, IConfiguration configuration)
    {
        _authClient = authClient;
        _db = db;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var data = new { nome = request.Nome, oab = request.OAB, telefone = request.Telefone };
        var res = await _authClient.SignUpAsync(request.Email, request.Password, data, cancellationToken);
        if (res?.AccessToken == null)
            return AuthResult.Fail("Falha no registro. Verifique os dados ou tente outro e-mail.");

        var userId = res.User?.Id;
        var advogadoId = Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        if (advogadoId != Guid.Empty)
        {
            var advogado = await _db.Advogados.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == advogadoId, cancellationToken);
            if (advogado != null)
            {
                advogado.OAB = request.OAB;
                advogado.Telefone = request.Telefone;
                advogado.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
        var profile = await BuildProfileFromTokenAsync(res.AccessToken, advogadoId, cancellationToken);
        return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Authenticate against local advogado table using PasswordHash
        var advogado = await _db.Advogados.AsNoTracking().FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);
        if (advogado == null || string.IsNullOrEmpty(advogado.PasswordHash))
        {
            // Fallback: try Supabase auth if configured (legacy)
            var res = await _authClient.TokenAsync(request.Email, request.Password, cancellationToken);
            if (res?.AccessToken == null)
                return AuthResult.Fail("E-mail ou senha inválidos.");
            var userId = res.User?.Id;
            Guid advogadoId = Guid.TryParse(userId, out var id) ? id : Guid.Empty;
            var profile = await BuildProfileFromTokenAsync(res.AccessToken, advogadoId, cancellationToken);
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
            return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
        }

        // Verify password
        var ok = PasswordHasher.Verify(request.Password, advogado.PasswordHash);
        if (!ok)
            return AuthResult.Fail("E-mail ou senha inválidos.");

        // Create JWT token for this advogado
        var jwtSecret = _configuration["JwtSettings:Secret"] ?? "";
        var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "";
        var jwtAudience = _configuration["JwtSettings:Audience"] ?? "authenticated";
        var expires = DateTimeOffset.UtcNow.AddHours(1);

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = string.IsNullOrEmpty(jwtSecret) ? null : new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = key == null ? null : new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, advogado.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, advogado.Nome),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, advogado.Email)
        };
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: string.IsNullOrEmpty(jwtIssuer) ? null : jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: creds);
        var accessToken = tokenHandler.WriteToken(jwt);

        var profileResp = new Application.Auth.DTOs.AdvogadoProfileResponse(advogado.Id, advogado.Nome, advogado.Email);
        return AuthResult.Ok(new AuthResponse(accessToken, string.Empty, expires, profileResp));
    }

    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var res = await _authClient.RefreshTokenAsync(refreshToken, cancellationToken);
        if (res?.AccessToken == null)
            return null;

        var userId = res.User?.Id;
        Guid advogadoId = Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        var profile = await BuildProfileFromTokenAsync(res.AccessToken, advogadoId, cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
        return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
    }

    public Task LogoutAsync(string accessToken, CancellationToken cancellationToken = default) =>
        _authClient.LogoutAsync(accessToken, cancellationToken);

    public Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default) =>
        _authClient.RecoverAsync(email, cancellationToken);

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var ok = await _authClient.UpdatePasswordAsync(token, newPassword, cancellationToken);
        if (!ok)
            throw new InvalidOperationException("Falha ao redefinir senha.");
    }

    public async Task<AdvogadoProfileResponse?> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var user = await _authClient.GetUserAsync(accessToken, cancellationToken);
        if (user?.Id == null || !Guid.TryParse(user.Id, out var advogadoId))
            return null;
        return await BuildProfileFromDbAsync(advogadoId, cancellationToken);
    }

    private async Task<AdvogadoProfileResponse?> BuildProfileFromTokenAsync(string accessToken, Guid advogadoId, CancellationToken ct)
    {
        var profile = await BuildProfileFromDbAsync(advogadoId, ct);
        return profile;
    }

    private async Task<AdvogadoProfileResponse?> BuildProfileFromDbAsync(Guid advogadoId, CancellationToken ct)
    {
        var adv = await _db.Advogados.IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == advogadoId, ct);
        if (adv == null) return null;
        return new AdvogadoProfileResponse(adv.Id, adv.Nome, adv.Email);
    }
}
