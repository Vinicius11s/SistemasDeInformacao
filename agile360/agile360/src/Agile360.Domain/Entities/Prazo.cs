namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: prazo
/// Representa um prazo jurídico vinculado obrigatoriamente a um Cliente e,
/// opcionalmente, a um Processo.
///
/// Serialização SnakeCaseLower (via SupabaseDataClient.JsonOpts):
///   IdProcesso       → id_processo
///   IdCliente        → id_cliente
///   DataVencimento   → data_vencimento
///   TipoContagem     → tipo_contagem
///   etc.
/// </summary>
public class Prazo : BaseEntity
{
    /// <summary>id_processo — uuid (opcional — FK para processo)</summary>
    public Guid? IdProcesso { get; set; }

    /// <summary>id_cliente — uuid (obrigatório — FK para cliente)</summary>
    public Guid IdCliente { get; set; }

    /// <summary>titulo — text NOT NULL</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>descricao — text</summary>
    public string? Descricao { get; set; }

    /// <summary>tipo_prazo — text (ex.: Recursal, Contestação, Petição…)</summary>
    public string? TipoPrazo { get; set; }

    /// <summary>prioridade — text DEFAULT 'Normal' (Baixa | Normal | Alta | Fatal)</summary>
    public string Prioridade { get; set; } = "Normal";

    /// <summary>data_publicacao — date (data da intimação/publicação)</summary>
    public DateOnly? DataPublicacao { get; set; }

    /// <summary>data_vencimento — date NOT NULL</summary>
    public DateOnly DataVencimento { get; set; }

    /// <summary>data_conclusao — timestamptz (preenchido ao concluir)</summary>
    public DateTimeOffset? DataConclusao { get; set; }

    /// <summary>status — text DEFAULT 'Pendente' (Pendente | Concluído | Cancelado)</summary>
    public string Status { get; set; } = "Pendente";

    /// <summary>criado_em — timestamptz DEFAULT now()</summary>
    public DateTimeOffset? CriadoEm { get; set; }

    /// <summary>lembrete_enviado — boolean DEFAULT false</summary>
    public bool LembreteEnviado { get; set; }

    /// <summary>tipo_contagem — text DEFAULT 'Util' (Util | Corrido)</summary>
    public string TipoContagem { get; set; } = "Util";

    /// <summary>prazo_dias — integer (base para cálculo da data_vencimento)</summary>
    public int? PrazoDias { get; set; }

    /// <summary>suspensao_prazos — boolean DEFAULT false</summary>
    public bool SuspensaoPrazos { get; set; }
}
