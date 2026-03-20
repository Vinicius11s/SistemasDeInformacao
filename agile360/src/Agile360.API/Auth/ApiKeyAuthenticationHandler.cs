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
            // [DIAG] Log leve para entender o que chega do n8n/Cloudflare (sem valores sensíveis)
            var headerKeys = string.Join(", ", Request.Headers.Keys);
            Logger.LogDebug("[ApiKey] {Method} {Path} | Headers: {Headers}",
                Request.Method, Request.Path, headerKeys);

            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var rawKeyValues))
            {
                // Defensive: a coleção de headers do ASP.NET já é case-insensitive,
                // mas garantimos aqui para cenários em que upstream (túnel/WAF)
                // pode reenviar com variação de capitalização.
                var match = Request.Headers.FirstOrDefault(h =>
                    h.Key.Equals(ApiKeyAuthenticationDefaults.HeaderName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(match.Key))
                {
                    Logger.LogDebug("[ApiKey] Header {Header} ausente. Deixando JWT assumir.",
                        ApiKeyAuthenticationDefaults.HeaderName);
                    return AuthenticateResult.NoResult(); // absent header → try JWT scheme next
                }

                rawKeyValues = match.Value;
            }

            var rawKey = rawKeyValues.FirstOrDefault()?.Trim(); // remove espaços invisíveis antes do hash
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                Logger.LogWarning("[ApiKey] Header {Header} vazio ou whitespace.",
                    ApiKeyAuthenticationDefaults.HeaderName);
                return AuthenticateResult.Fail("X-Api-Key header is empty.");
            }

            var safePrefix = rawKey.Length >= 8 ? rawKey[..8] : "??";
            var hash = TokenHasher.Hash(rawKey);

            Logger.LogInformation("[ApiKey] Header recebido. prefixo={Prefix} | {Method} {Path}",
                safePrefix, Request.Method, Request.Path);

            var hashPrefix = hash.Length >= 8 ? hash[..8] : hash;
            Logger.LogDebug("[ApiKey] Tentando autenticar chave com prefixo {Prefix} | hash_prefix={HashPrefix}",
                safePrefix, hashPrefix);

            var repo = Context.RequestServices.GetRequiredService<IApiKeyRepository>();
            var apiKey = await repo.FindActiveAsync(hash, Context.RequestAborted);

            if (apiKey == null)
            {
                Logger.LogWarning("[ApiKey] Chave inválida ou expirada (prefixo: {Prefix})", safePrefix);
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
            new Claim("api_key_name", apiKey.KeyPrefix),
        };

        var identity  = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, ApiKeyAuthenticationDefaults.AuthenticationScheme);

        Logger.LogInformation("API key '{KeyPrefix}' authenticated for AdvogadoId {AdvogadoId}",
            apiKey.KeyPrefix, apiKey.AdvogadoId);

        return AuthenticateResult.Success(ticket);
    }
}
