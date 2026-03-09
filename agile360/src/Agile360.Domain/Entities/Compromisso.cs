namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: compromisso
/// Schema real: id_advogado, id_cliente, id_processo, criado_em (date), tipo_compromisso, etc.
/// </summary>
public class Compromisso : BaseEntity
{
    // tipo_compromisso
    public string TipoCompromisso { get; set; } = string.Empty;

    // tipo_audiencia
    public string? TipoAudiencia { get; set; }

    // is_active (bool) — herdado de BaseEntity, mapeado para coluna is_active

    // data
    public DateOnly Data { get; set; }

    // hora
    public TimeOnly Hora { get; set; }

    // local
    public string? Local { get; set; }

    // id_cliente
    public Guid? ClienteId { get; set; }

    // id_processo
    public Guid? ProcessoId { get; set; }

    // observacoes
    public string? Observacoes { get; set; }

    // lembrete_minutos
    public int? LembreteMinutos { get; set; }

    // criado_em (date — não timestamptz)
    public DateOnly CriadoEm { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
