namespace Agile360.Domain.Entities;

/// <summary>
/// Tenant root — cada advogado é um tenant.
/// Mapeado para a tabela pública "advogado" no Supabase/PostgreSQL.
/// O Id é o mesmo UID do Supabase Auth (auth.users.id).
/// </summary>
public class Advogado
{
    // ─── Identidade ──────────────────────────────────────────────────────────────

    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Role { get; set; }

    // ─── OAB / Profissional ───────────────────────────────────────────────────────

    /// <summary>Número OAB com UF (ex.: "SP123456")</summary>
    public string? OAB { get; set; }

    // ─── Contato ─────────────────────────────────────────────────────────────────

    public string? Telefone { get; set; }

    /// <summary>Identificador do canal WhatsApp no n8n/Evolution API</summary>
    public string? WhatsAppId { get; set; }

    // ─── Perfil / Escritório ──────────────────────────────────────────────────────

    public string? FotoUrl { get; set; }
    public string? NomeEscritorio { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    // ─── Assinatura ───────────────────────────────────────────────────────────────

    public string? Plano { get; set; }
    public string? StatusAssinatura { get; set; }

    /// <summary>
    /// Data de expiração do plano. Armazenada como tipo "date" no PostgreSQL
    /// (sem fuso horário). Usar DateOnly evita InvalidCastException do Npgsql.
    /// </summary>
    public DateOnly? DataExpiracao { get; set; }
    public string? StripeCustomerId { get; set; }

    // ─── Auth local ───────────────────────────────────────────────────────────────

    /// <summary>Hash da senha para autenticação local (PBKDF2). Nullable para usuários Supabase-only.</summary>
    public string? PasswordHash { get; set; }

    // ─── MFA (Google Authenticator) ───────────────────────────────────────────────

    public bool MfaEnabled { get; set; } = false;

    /// <summary>Segredo TOTP ativo — criptografado com AES-256-GCM.</summary>
    public string? MfaSecret { get; set; }

    /// <summary>Segredo TOTP pendente de confirmação (setup flow).</summary>
    public string? MfaPendingSecret { get; set; }

    // ─── Auditoria ───────────────────────────────────────────────────────────────

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
