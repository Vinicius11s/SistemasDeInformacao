namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: processo
/// Schema real: id_advogado, id_cliente, num_processo, status (text), criado_em (date), etc.
/// </summary>
public class Processo : BaseEntity
{
    // id_cliente
    public Guid? ClienteId { get; set; }

    // num_processo
    public string NumProcesso { get; set; } = string.Empty;

    // parte_contraria
    public string? ParteContraria { get; set; }

    // tribunal
    public string? Tribunal { get; set; }

    // comarca_vara
    public string? ComarcaVara { get; set; }

    // assunto
    public string? Assunto { get; set; }

    // valor_causa
    public decimal? ValorCausa { get; set; }

    // honorarios_estimados
    public decimal? HonorariosEstimados { get; set; }

    // fase_processual
    public string? FaseProcessual { get; set; }

    // status
    public string Status { get; set; } = "Ativo";

    // data_distribuicao
    public DateOnly? DataDistribuicao { get; set; }

    // criado_em (date — não timestamptz)
    public DateOnly CriadoEm { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // observacoes
    public string? Observacoes { get; set; }

    // ─── Aliases para compatibilidade com código existente ──────────────────
    public string? Comarca => ComarcaVara;
    public string? TipoAcao => Assunto;
    public string NumeroProcesso => NumProcesso;
}
