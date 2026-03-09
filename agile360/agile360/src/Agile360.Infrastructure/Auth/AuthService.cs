using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Infrastructure.Data;

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

    /// <summary>
    /// Nome real da tabela no Supabase (conforme configuração da entidade).
    /// URL resultante: /rest/v1/advogado
    /// </summary>
    private const string AdvogadosTable = "advogado";

    public AuthService(SupabaseAuthClient authClient, SupabaseDataClient dataClient)
    {
        _authClient = authClient;
        _dataClient = dataClient;
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

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // ──────────────────────────────────────────────────────────────────────
        // Passo 1 — Autenticação
        //   POST https://<project>.supabase.co/auth/v1/token?grant_type=password
        //   Headers: apikey: <AnonKey>
        //   Body:    { email, password }
        // ──────────────────────────────────────────────────────────────────────
        var res = await _authClient.TokenAsync(request.Email, request.Password, ct);
        if (res?.AccessToken == null)
            return AuthResult.Fail("E-mail ou senha inválidos.");

        // ──────────────────────────────────────────────────────────────────────
        // Passo 2 — O "Crachá": Supabase devolveu um AccessToken (JWT)
        //   res.AccessToken = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        // ──────────────────────────────────────────────────────────────────────
        Guid advogadoId = Guid.TryParse(res.User?.Id, out var id) ? id : Guid.Empty;

        // ──────────────────────────────────────────────────────────────────────
        // Passo 3 — Consulta de dados usando o AccessToken
        //   GET https://<project>.supabase.co/rest/v1/advogado?Id=eq.{id}
        //   Headers: apikey: <AnonKey>
        //            Authorization: Bearer <AccessToken>   ← o "crachá"
        // ──────────────────────────────────────────────────────────────────────
        var profile   = await GetProfileByIdAsync(advogadoId, res.AccessToken, ct);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn);

        return AuthResult.Ok(new AuthResponse(res.AccessToken, res.RefreshToken ?? "", expiresAt, profile!));
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
            NumeroOab:        adv.NumeroOab,
            OabUf:            adv.OabUf,
            NomeEscritorio:   adv.NomeEscritorio,
            Plano:            adv.Plano,
            StatusAssinatura: adv.StatusAssinatura,
            LogoUrl:          adv.LogoUrl,
            TelefoneContato:  adv.TelefoneContato,
            Cidade:           adv.Cidade,
            Estado:           adv.Estado,
            DataExpiracao:    adv.DataExpiracao);
    }
}
