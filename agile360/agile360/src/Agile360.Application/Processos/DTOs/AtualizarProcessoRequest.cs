namespace Agile360.Application.Processos.DTOs;

public record AtualizarProcessoRequest(
    Guid     IdCliente,
    string   NumProcesso,
    string   Status,
    string?  ParteContraria,
    string?  Tribunal,
    string?  ComarcaVara,
    string?  Assunto,
    decimal? ValorCausa,
    decimal? HonorariosEstimados,
    string?  FaseProcessual,
    DateOnly? DataDistribuicao,
    string?  Observacoes
);
