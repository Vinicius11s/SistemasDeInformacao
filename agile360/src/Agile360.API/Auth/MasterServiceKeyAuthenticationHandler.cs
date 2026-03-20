using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agile360.API.Auth;

public static class MasterServiceKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "MasterServiceKey";

    // Env var: MasterServiceKey (backend)
    // Header: X-Master-Service-Key (n8n)
    public const string MasterKeyHeaderName = "X-Master-Service-Key";

    // Header: UUID do advogado alvo no modelo multi-tenant
    public const string OnBehalfOfHeaderName = "X-On-Behalf-Of";
}

public class MasterServiceKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    // Valor esperado vem de Env Var / Configuration.
    public string ExpectedMasterKey { get; set; } = string.Empty;
}

/// <summary>
/// Auth “Master Key” (para n8n / Hub Central).
/// - Valida X-Master-Service-Key contra a Env Var MasterServiceKey.
/// - Se válido, exige X-On-Behalf-Of (UUID) e injeta claims para o tenant.
/// </summary>
public class MasterServiceKeyAuthenticationHandler
    : AuthenticationHandler<MasterServiceKeyAuthenticationOptions>
{
    public MasterServiceKeyAuthenticationHandler(
        IOptionsMonitor<MasterServiceKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Se o header não existe, não interfere: deixa JWT / ApiKey tentarem.
        if (!Request.Headers.TryGetValue(
                MasterServiceKeyAuthenticationDefaults.MasterKeyHeaderName,
                out var providedMasterKeyValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var providedMasterKey = providedMasterKeyValues.FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(providedMasterKey))
            return Task.FromResult(AuthenticateResult.Fail(
                "X-Master-Service-Key header is empty."));

        if (string.IsNullOrWhiteSpace(Options.ExpectedMasterKey))
            return Task.FromResult(AuthenticateResult.Fail(
                "Master service key não configurada no servidor."));

        if (!FixedTimeEqualsUtf8(providedMasterKey, Options.ExpectedMasterKey))
            return Task.FromResult(AuthenticateResult.Fail(
                "Master service key inválida."));

        if (!Request.Headers.TryGetValue(
                MasterServiceKeyAuthenticationDefaults.OnBehalfOfHeaderName,
                out var onBehalfOfValues))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"{MasterServiceKeyAuthenticationDefaults.OnBehalfOfHeaderName} header ausente."));
        }

        var onBehalfOf = onBehalfOfValues.FirstOrDefault()?.Trim();
        if (!Guid.TryParse(onBehalfOf, out var advogadoId))
            return Task.FromResult(AuthenticateResult.Fail(
                $"{MasterServiceKeyAuthenticationDefaults.OnBehalfOfHeaderName} inválido."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, advogadoId.ToString()),
            new Claim("sub",                    advogadoId.ToString()),
            new Claim("advogado_id",           advogadoId.ToString()),
            new Claim("auth_method",          "master_key"),
        };

        var identity  = new ClaimsIdentity(
            claims,
            MasterServiceKeyAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(
            principal,
            MasterServiceKeyAuthenticationDefaults.AuthenticationScheme);

        Logger.LogInformation(
            "[MasterKey] authenticated for AdvogadoId {AdvogadoId}",
            advogadoId);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool FixedTimeEqualsUtf8(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        if (aBytes.Length != bBytes.Length) return false;
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}

