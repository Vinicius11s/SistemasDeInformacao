namespace Agile360.Domain.Entities;

/// <summary>
/// Tenant root — cada advogado é um tenant.
/// Mapeado para a tabela pública "advogado" no Supabase.
/// O Id é o mesmo UID do Supabase Auth (auth.users.id).
///
/// Convenção de serialização: JsonNamingPolicy.SnakeCaseLower converte
/// automaticamente as propriedades C# para as colunas snake_case do PostgreSQL:
///   NumeroOab  → numero_oab
///   OabUf      → oab_uf
///   RemoteJid  → remote_jid   … etc.
/// </summary>
public class Advogado
{
    // ─── Identidade ──────────────────────────────────────────────────────────────

    /// <summary>id — uuid (PK, mesmo UID do Supabase Auth)</summary>
    public Guid Id { get; set; }

    /// <summary>email — text</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>nome — text</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>role — text (ex.: "advogado", "admin")</summary>
    public string? Role { get; set; }

    // ─── OAB ─────────────────────────────────────────────────────────────────────

    /// <summary>numero_oab — text</summary>
    public string? NumeroOab { get; set; }

    /// <summary>oab_uf — text, máx. 2 chars (ex.: "SP", "RJ")</summary>
    public string? OabUf { get; set; }

    // ─── Escritório ───────────────────────────────────────────────────────────────

    /// <summary>nome_escritorio — text</summary>
    public string? NomeEscritorio { get; set; }

    /// <summary>telefone_contato — text</summary>
    public string? TelefoneContato { get; set; }

    /// <summary>cidade — text</summary>
    public string? Cidade { get; set; }

    /// <summary>estado — text</summary>
    public string? Estado { get; set; }

    /// <summary>cpf_cnpj — text</summary>
    public string? CpfCnpj { get; set; }

    /// <summary>logo_url — text</summary>
    public string? LogoUrl { get; set; }

    // ─── WhatsApp / Canal ─────────────────────────────────────────────────────────

    /// <summary>remote_jid — text (identificador do canal WhatsApp no n8n/Evolution API)</summary>
    public string? RemoteJid { get; set; }

    // ─── Assinatura ───────────────────────────────────────────────────────────────

    /// <summary>plano — text (ex.: "free", "pro", "enterprise")</summary>
    public string? Plano { get; set; }

    /// <summary>status_assinatura — text (ex.: "ativa", "cancelada", "trial")</summary>
    public string? StatusAssinatura { get; set; }

    /// <summary>data_expiracao — date</summary>
    public DateTimeOffset? DataExpiracao { get; set; }

    /// <summary>stripe_customer_id — text</summary>
    public string? StripeCustomerId { get; set; }

    // ─── Auditoria ───────────────────────────────────────────────────────────────

    /// <summary>created_at — date</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
