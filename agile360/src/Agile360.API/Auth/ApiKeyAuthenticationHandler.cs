using System.Security.Claims;
using System.Text.Encodings.Web;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agile360.API.Auth;

public static class ApiKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions { }

/// <summary>
/// Custom ASP.NET Core authentication scheme that validates the X-Api-Key header.
///
/// Flow per request:
///   1. Read X-Api-Key header — if absent, return NoResult() so JWT scheme is tried next
///   2. SHA-256 hash the raw key (TokenHasher)
///   3. Look up the hash in api_keys table (active, not expired, not revoked)
///   4. Build ClaimsPrincipal with AdvogadoId as sub — same claims shape as JWT
///   5. Fire-and-forget: update last_used_at
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var rawKeyValues))
            return AuthenticateResult.NoResult(); // absent header → try JWT scheme next

        var rawKey = rawKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.Fail("X-Api-Key header is empty.");

        var repo = Context.RequestServices.GetRequiredService<IApiKeyRepository>();
        var hash = TokenHasher.Hash(rawKey);
        var apiKey = await repo.FindActiveAsync(hash, Context.RequestAborted);

        if (apiKey == null)
        {
            Logger.LogWarning("Invalid or expired API key (prefix: {Prefix})",
                rawKey.Length >= 8 ? rawKey[..8] : "??");
            return AuthenticateResult.Fail("API key inválida ou expirada.");
        }

        // Fire-and-forget com escopo próprio: evita usar o DbContext do request após ele ser descartado.
        var scopeFactory = Context.RequestServices.GetRequiredService<IServiceScopeFactory>();
        var keyId = apiKey.Id;
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var r = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
                await r.TouchAsync(keyId);
            }
            catch
            {
                // Não-crítico: falha em atualizar last_used_at não deve interromper o fluxo.
            }
        });

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKey.AdvogadoId.ToString()),
            new Claim("sub",          apiKey.AdvogadoId.ToString()),
            new Claim("advogado_id",  apiKey.AdvogadoId.ToString()),
            new Claim("auth_method",  "api_key"),
            new Claim("api_key_id",   apiKey.Id.ToString()),
            new Claim("api_key_name", apiKey.Name),
        };

        var identity  = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, ApiKeyAuthenticationDefaults.AuthenticationScheme);

        Logger.LogInformation("API key '{Name}' authenticated for AdvogadoId {AdvogadoId}",
            apiKey.Name, apiKey.AdvogadoId);

        return AuthenticateResult.Success(ticket);
    }
}
