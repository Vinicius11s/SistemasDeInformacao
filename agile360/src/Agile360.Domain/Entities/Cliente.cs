using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: cliente
/// Suporta Pessoa Física (tipo_cliente = 'Pessoa Física') e
/// Pessoa Jurídica (tipo_cliente = 'Pessoa Jurídica').
/// </summary>
public class Cliente : BaseEntity
{
    // tipo_cliente
    public string TipoCliente { get; set; } = "Pessoa Física";

    // ─── Pessoa Física ──────────────────────────────────────────────────────
    public string? NomeCompleto { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public string? OrgaoExpedidor { get; set; }

    // ─── Pessoa Jurídica ────────────────────────────────────────────────────
    public string? RazaoSocial { get; set; }
    public string? CNPJ { get; set; }
    public string? InscricaoEstadual { get; set; }

    // ─── Contato ────────────────────────────────────────────────────────────
    public string? Telefone { get; set; }

    // ─── Endereço ───────────────────────────────────────────────────────────
    public string? CEP { get; set; }
    public string? Estado { get; set; }
    public string? Cidade { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Bairro { get; set; }
    public string? Complemento { get; set; }

    // ─── Dados adicionais ───────────────────────────────────────────────────
    public DateOnly? DataReferencia { get; set; }
    public string? EstadoCivil { get; set; }
    public string? AreaAtuacao { get; set; }
    public string? NumeroConta { get; set; }
    public string? Pix { get; set; }
    // is_active (bool) — substitui o antigo status texto; herdado de BaseEntity e mapeado em cliente.is_active
    public DateOnly DataCadastro { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // ─── Helpers ────────────────────────────────────────────────────────────
    /// <summary>Nome exibível independente do tipo de pessoa.</summary>
    public string NomeExibicao => NomeCompleto ?? RazaoSocial ?? "(sem nome)";

    /// <summary>Compatibilidade com código que usa TipoPessoa enum.</summary>
    public TipoPessoa TipoPessoa => TipoCliente == "Pessoa Jurídica"
        ? TipoPessoa.PessoaJuridica
        : TipoPessoa.PessoaFisica;

    /// <summary>Documento principal (CPF ou CNPJ).</summary>
    public string? Documento => CPF ?? CNPJ;

    public string? Observacoes { get; set; }
    public OrigemCliente Origem { get; set; } = OrigemCliente.Manual;
}
