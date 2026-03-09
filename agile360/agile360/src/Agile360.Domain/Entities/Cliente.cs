namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: cliente
/// Serialização: JsonNamingPolicy.SnakeCaseLower converte automaticamente
///   NomeCompleto → nome_completo, OrgaoExpedidor → orgao_expedidor, etc.
/// </summary>
public class Cliente : BaseEntity
{
    // ─── Identificação ───────────────────────────────────────────────────────────
    /// <summary>tipo_cliente — text ('Pessoa Física' | 'Pessoa Jurídica')</summary>
    public string TipoCliente { get; set; } = "Pessoa Física";

    /// <summary>nome_completo — text</summary>
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>cpf — text</summary>
    public string? Cpf { get; set; }

    /// <summary>rg — text</summary>
    public string? Rg { get; set; }

    /// <summary>orgao_expedidor — text</summary>
    public string? OrgaoExpedidor { get; set; }

    /// <summary>data_nascimento — date</summary>
    public DateOnly? DataNascimento { get; set; }

    /// <summary>estado_civil — text</summary>
    public string? EstadoCivil { get; set; }

    /// <summary>profissao — text</summary>
    public string? Profissao { get; set; }

    // ─── Contato ─────────────────────────────────────────────────────────────────
    /// <summary>telefone — text</summary>
    public string? Telefone { get; set; }

    // ─── Financeiro ───────────────────────────────────────────────────────────────
    /// <summary>numero_conta — text</summary>
    public string? NumeroConta { get; set; }

    /// <summary>pix — text</summary>
    public string? Pix { get; set; }

    // ─── Endereço ────────────────────────────────────────────────────────────────
    /// <summary>cep — text</summary>
    public string? Cep { get; set; }

    /// <summary>endereco — text (logradouro)</summary>
    public string? Endereco { get; set; }

    /// <summary>numero — text</summary>
    public string? Numero { get; set; }

    /// <summary>bairro — text</summary>
    public string? Bairro { get; set; }

    /// <summary>complemento — text</summary>
    public string? Complemento { get; set; }

    /// <summary>cidade — text</summary>
    public string? Cidade { get; set; }

    /// <summary>estado — text (max 2 chars, ex.: "SP")</summary>
    public string? Estado { get; set; }

    // ─── Auditoria ───────────────────────────────────────────────────────────────
    /// <summary>data_cadastro — date (DEFAULT now() no Supabase)</summary>
    public DateOnly? DataCadastro { get; set; }
}
