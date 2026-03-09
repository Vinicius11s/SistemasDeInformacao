namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: prazo
/// Schema real: id_advogado, id_processo, id_cliente, titulo, descricao, tipo_prazo,
/// prioridade, data_vencimento, data_conclusao, status, criado_em, lembrete_enviado,
/// tipo_contagem, prazo_dias, suspensao_prazos, data_publicacao.
/// </summary>
public class Prazo : BaseEntity
{
    // id_processo
    public Guid? ProcessoId { get; set; }

    // id_cliente
    public Guid? ClienteId { get; set; }

    // titulo
    public string Titulo { get; set; } = string.Empty;

    // descricao
    public string? Descricao { get; set; }

    // tipo_prazo
    public string TipoPrazo { get; set; } = "Ordinário";

    // prioridade
    public string Prioridade { get; set; } = "Normal";

    // status
    public string Status { get; set; } = "Pendente";

    // data_publicacao
    public DateOnly? DataPublicacao { get; set; }

    // data_vencimento
    public DateOnly DataVencimento { get; set; }

    // data_conclusao
    public DateTimeOffset? DataConclusao { get; set; }

    // criado_em
    public DateTimeOffset? CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    // lembrete_enviado
    public bool LembreteEnviado { get; set; } = false;

    // tipo_contagem
    public string TipoContagem { get; set; } = "Util";

    // prazo_dias
    public int? PrazoDias { get; set; }

    // suspensao_prazos
    public bool SuspensaoPrazos { get; set; } = false;

    // ─── Aliases para compatibilidade ───────────────────────────────────────
    public string Descricao_ => Titulo;
}
