namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: processo
/// Serialização SnakeCaseLower:
///   IdCliente → id_cliente, NumProcesso → num_processo, etc.
/// </summary>
public class Processo : BaseEntity
{
    /// <summary>id_cliente — uuid (FK para cliente)</summary>
    public Guid IdCliente { get; set; }

    /// <summary>num_processo — text (ex.: 0000000-00.2026.8.26.0000)</summary>
    public string NumProcesso { get; set; } = string.Empty;

    /// <summary>status — text (Ativo, Suspenso, Arquivado, Encerrado)</summary>
    public string Status { get; set; } = "Ativo";

    /// <summary>parte_contraria — text</summary>
    public string? ParteContraria { get; set; }

    /// <summary>tribunal — text (TJSP, TRT2, JFPR, STF…)</summary>
    public string? Tribunal { get; set; }

    /// <summary>comarca_vara — text (ex.: 2ª Vara Cível de Presidente Prudente)</summary>
    public string? ComarcaVara { get; set; }

    /// <summary>assunto — text (Danos Morais, Reclamação Trabalhista…)</summary>
    public string? Assunto { get; set; }

    /// <summary>valor_causa — numeric</summary>
    public decimal? ValorCausa { get; set; }

    /// <summary>honorarios_estimados — numeric</summary>
    public decimal? HonorariosEstimados { get; set; }

    /// <summary>fase_processual — text (Conhecimento, Recursal, Execução)</summary>
    public string? FaseProcessual { get; set; }

    /// <summary>data_distribuicao — date</summary>
    public DateOnly? DataDistribuicao { get; set; }

    /// <summary>observacoes — text</summary>
    public string? Observacoes { get; set; }

    /// <summary>criado_em — date (DEFAULT now() no Supabase)</summary>
    public DateOnly? CriadoEm { get; set; }
}
