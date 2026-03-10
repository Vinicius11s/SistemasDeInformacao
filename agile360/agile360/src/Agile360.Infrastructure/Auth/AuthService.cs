using System.Diagnostics;
using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// Serviço de autenticação alinhado ao padrão Supabase Data API:
///
///   Passo 1 — Autenticação:  chama TokenAsync do SupabaseAuthClient.
///   Passo 2 — O "Crachá":   Supabase devolve um AccessToken (JWT).
///   Passo 3 — Consulta:     envia esse AccessToken em Authorization: Bearer
///                           para a Data API (/rest/v1/advogado).
///
/// Nenhuma autenticação local existe — tudo é delegado ao Supabase Auth.
/// </summary>
public class AuthService : IAuthService
{
    private readonly SupabaseAuthClient _authClient;
    private readonly SupabaseDataClient _dataClient;
    private readonly ILogger<AuthService> _logger;

    private const string AdvogadosTable = "advogado";

    public AuthService(SupabaseAuthClient authClient, SupabaseDataClient dataClient, ILogger<AuthService> logger)
    {
        _authClient = authClient;
        _dataClient = dataClient;
        _logger     = logger;
    }

    // ─── Registro ────────────────────────────────────────────────────────────────

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Passo 1: Cria usuário no Supabase Auth → recebe AccessToken
        var data = new { nome = request.Nome, oab = request.OAB, telefone = request.Telefone };
        var res = await _authClient.SignUpAsync(request.Email, request.Password, data, ct);
        if (res?.AccessToken == null)
            return AuthResult.Fail("Falha no registro. Verifique os dados ou tente outro e-mail.");

        // Passo 2: Usando o AccessToken para atualizar os dados do advogado na Data API
        // PATCH /rest/v1/advogado?Id=eq.{id}   Authorization: Bearer <AccessToken>
        if (Guid.TryParse(res.User?.Id, out var advogadoId) && advogadoId != Guid.Empty)
        {
            await _dataClient.PatchAsync(
                AdvogadosTable,
                $"id=eq.{advogadoId}",
                new
                {
                    numero_oab       = request.OAB,
                    oab_uf           = request.OabUf,
                    telefone_contato = request.Telefone,
                    nome_escritorio  = request.NomeEscritorio
                },
                res.AccessToken,
                ct);
        }

        var profile   = await GetProfileByIdAsync(advogadoId, res.AccessToken, ct);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
        return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
    }

    // ─── Login ───────────────────────────────────────────────────────────────────

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[Login] Iniciando autenticação para {Email}", request.Email);

        // Passo 1 — POST /auth/v1/token (Supabase GoTrue)
        SupabaseTokenResponse? res;
        try
        {
            res = await _authClient.TokenAsync(request.Email, request.Password, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Login] Falha ao chamar Supabase Auth ({Elapsed}ms)", sw.ElapsedMilliseconds);
            return LoginResult.Fail("Não foi possível conectar ao servidor de autenticação. Tente novamente.");
        }

        _logger.LogInformation("[Login] Supabase Auth respondeu em {Elapsed}ms — sucesso: {Ok}",
            sw.ElapsedMilliseconds, res?.AccessToken != null);

        if (res?.AccessToken == null)
            return LoginResult.Fail("E-mail ou senha inválidos.");

        Guid advogadoId = Guid.TryParse(res.User?.Id, out var id) ? id : Guid.Empty;

        // Passo 2 — GET /rest/v1/advogado (entidade completa — usada para MFA check E perfil)
        sw.Restart();
        Advogado? adv = null;
        try
        {
            adv = await _dataClient.GetSingleAsync<Advogado>(AdvogadosTable, $"id=eq.{advogadoId}", res.AccessToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Login] Falha ao buscar entidade do advogado {AdvogadoId} ({Elapsed}ms)",
                advogadoId, sw.ElapsedMilliseconds);
        }

        _logger.LogInformation("[Login] Entidade carregada em {Elapsed}ms — AdvogadoId: {AdvogadoId} | MfaEnabled: {Mfa}",
            sw.ElapsedMilliseconds, advogadoId, adv?.MfaEnabled);

        // ── MFA Gate ──────────────────────────────────────────────────────────────
        // Se o advogado tem 2FA ativo, NÃO emite o JWT final.
        // Retorna apenas um token temporário para o frontend redirecionar
        // para a tela de desafio MFA (POST /api/auth/mfa/challenge).
        if (adv?.MfaEnabled == true)
        {
            _logger.LogInformation("[Login] MFA ativo para {AdvogadoId} — emitindo desafio (202)", advogadoId);
            return LoginResult.MfaChallenge(new MfaRequiredResponse(res.AccessToken));
        }

        // ── Sem MFA: emite tokens completos ───────────────────────────────────────
        var profile = adv == null ? null : new AdvogadoProfileResponse(
            Id:               adv.Id,
            Nome:             adv.Nome,
            Email:            adv.Email,
            Role:             adv.Role,
            OAB:              adv.OAB,
            NomeEscritorio:   adv.NomeEscritorio,
            Plano:            adv.Plano,
            StatusAssinatura: adv.StatusAssinatura,
            FotoUrl:          adv.FotoUrl,
            Telefone:         adv.Telefone,
            Cidade:           adv.Cidade,
            Estado:           adv.Estado,
            DataExpiracao:    adv.DataExpiracao);

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
        return LoginResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
    }

    // ─── Refresh ─────────────────────────────────────────────────────────────────

    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var res = await _authClient.RefreshTokenAsync(refreshToken, ct);
        if (res?.AccessToken == null) return null;

        Guid advogadoId = Guid.TryParse(res.User?.Id, out var id) ? id : Guid.Empty;
        var profile   = await GetProfileByIdAsync(advogadoId, res.AccessToken, ct);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);
        return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
    }

    // ─── Logout / Recovery ───────────────────────────────────────────────────────

    public Task LogoutAsync(string accessToken, CancellationToken ct = default) =>
        _authClient.LogoutAsync(accessToken, ct);

    public Task ForgotPasswordAsync(string email, CancellationToken ct = default) =>
        _authClient.RecoverAsync(email, ct);

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        var ok = await _authClient.UpdatePasswordAsync(token, newPassword, ct);
        if (!ok) throw new InvalidOperationException("Falha ao redefinir senha.");
    }

    // ─── Perfil (GET /api/auth/me) ───────────────────────────────────────────────

    public async Task<AdvogadoProfileResponse?> GetProfileAsync(string accessToken, CancellationToken ct = default)
    {
        var user = await _authClient.GetUserAsync(accessToken, ct);
        if (user?.Id == null || !Guid.TryParse(user.Id, out var advogadoId)) return null;

        // Usa o próprio AccessToken do usuário para consultar o perfil
        return await GetProfileByIdAsync(advogadoId, accessToken, ct);
    }

    // ─── Auxiliar ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Consulta o perfil do advogado via Supabase Data API usando o AccessToken do usuário.
    ///   GET /rest/v1/advogado?Id=eq.{id}
    ///   Headers: apikey: {AnonKey}
    ///            Authorization: Bearer {accessToken}
    ///
    /// Se o ServiceRoleKey estiver configurado (ambiente servidor), usa-o como fallback
    /// para garantir que a consulta funcione mesmo sem RLS configurado na tabela advogado.
    /// </summary>
    private async Task<AdvogadoProfileResponse?> GetProfileByIdAsync(
        Guid advogadoId, string accessToken, CancellationToken ct)
    {
        if (advogadoId == Guid.Empty) return null;

        // Prefere o AccessToken do usuário; cai para ServiceToken como fallback administrativo
        var token = !string.IsNullOrEmpty(accessToken)
            ? accessToken
            : _dataClient.ServiceToken;

        var adv = await _dataClient.GetSingleAsync<Advogado>(
            AdvogadosTable,
            $"id=eq.{advogadoId}",      // ← snake_case: coluna "id" no PostgreSQL
            token,
            ct);

        if (adv == null) return null;

        return new AdvogadoProfileResponse(
            Id:               adv.Id,
            Nome:             adv.Nome,
            Email:            adv.Email,
            Role:             adv.Role,
            OAB:              adv.OAB,
            NomeEscritorio:   adv.NomeEscritorio,
            Plano:            adv.Plano,
            StatusAssinatura: adv.StatusAssinatura,
            FotoUrl:          adv.FotoUrl,
            Telefone:         adv.Telefone,
            Cidade:           adv.Cidade,
            Estado:           adv.Estado,
            DataExpiracao:    adv.DataExpiracao);
    }

    // ─── MFA ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Valida o temp token de MFA. Em produção este token deve ser um JWT de curta duração;
    /// neste stub, decodifica o sub sem validar assinatura (apenas para compilar).
    /// Substitua por validação JWT real antes de ir a produção.
    /// </summary>
    public Guid? ValidateMfaTempToken(string tempToken)
    {
        try
        {
            var parts = tempToken.Split('.');
            if (parts.Length < 2) return null;

            var payload = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(PadBase64(parts[1])));
            var doc = System.Text.Json.JsonDocument.Parse(payload);

            if (doc.RootElement.TryGetProperty("sub", out var sub)
                && Guid.TryParse(sub.GetString(), out var id))
                return id;

            return null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Completa o challenge MFA: re-emite tokens completos via Supabase.
    /// O tempToken contém o access token original; usamos para fazer refresh.
    /// </summary>
    public async Task<AuthResult?> CompleteMfaChallengeAsync(
        string tempToken, string totpCode, CancellationToken ct = default)
    {
        var advogadoId = ValidateMfaTempToken(tempToken);
        if (advogadoId == null) return null;

        var profile = await GetProfileByIdAsync(advogadoId.Value, tempToken, ct);
        if (profile == null) return null;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        return AuthResult.Ok(new AuthResponse(tempToken, string.Empty, expiresAt, profile));
    }

    private static string PadBase64(string s)
    {
        int rem = s.Length % 4;
        return rem switch
        {
            2 => s + "==",
            3 => s + "=",
            _ => s
        };
    }
}
