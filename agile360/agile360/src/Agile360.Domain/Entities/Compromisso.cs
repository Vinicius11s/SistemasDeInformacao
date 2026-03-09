namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: compromisso (anteriormente chamada de "audiencia" no código)
/// Serialização SnakeCaseLower:
///   TipoCompromisso → tipo_compromisso, LembreteMinutos → lembrete_minutos, etc.
/// </summary>
public class Compromisso : BaseEntity
{
    /// <summary>tipo_compromisso — text (Audiência, Atendimento, Reunião, Prazo)</summary>
    public string TipoCompromisso { get; set; } = string.Empty;

    /// <summary>tipo_audiencia — text (Conciliação, Instrução e Julgamento…) — só se tipo = Audiência</summary>
    public string? TipoAudiencia { get; set; }

    /// <summary>status — text (Agendado, Concluído, Cancelado)</summary>
    public string Status { get; set; } = "Agendado";

    /// <summary>data — date</summary>
    public DateOnly Data { get; set; }

    /// <summary>hora — time</summary>
    public TimeOnly Hora { get; set; }

    /// <summary>local — text (ex.: Fórum Cível - Sala 402)</summary>
    public string? Local { get; set; }

    /// <summary>id_cliente — uuid (opcional)</summary>
    public Guid? IdCliente { get; set; }

    /// <summary>id_processo — uuid (obrigatório se tipo = Audiência)</summary>
    public Guid? IdProcesso { get; set; }

    /// <summary>observacoes — text</summary>
    public string? Observacoes { get; set; }

    /// <summary>lembrete_minutos — int (ex.: 60 = avisar 1h antes)</summary>
    public int? LembreteMinutos { get; set; }

    /// <summary>criado_em — date (DEFAULT now() no Supabase)</summary>
    public DateOnly? CriadoEm { get; set; }
}
