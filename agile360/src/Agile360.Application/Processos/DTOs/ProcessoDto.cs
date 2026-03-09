namespace Agile360.Application.Processos.DTOs;

public record ProcessoResponse(
    Guid      Id,
    Guid?     ClienteId,
    string    NumProcesso,
    string?   ParteContraria,
    string?   Tribunal,
    string?   ComarcaVara,
    string?   Assunto,
    decimal?  ValorCausa,
    decimal?  HonorariosEstimados,
    string?   FaseProcessual,
    string    Status,
    DateOnly? DataDistribuicao,
    DateOnly  CriadoEm,
    string?   Observacoes
);

public record CreateProcessoRequest(
    Guid?     ClienteId,
    string    NumProcesso,
    string?   ParteContraria,
    string?   Tribunal,
    string?   ComarcaVara,
    string?   Assunto,
    decimal?  ValorCausa,
    decimal?  HonorariosEstimados,
    string?   FaseProcessual,
    string    Status,
    DateOnly? DataDistribuicao,
    string?   Observacoes
);

public record UpdateProcessoRequest(
    Guid?     ClienteId,
    string?   NumProcesso,
    string?   ParteContraria,
    string?   Tribunal,
    string?   ComarcaVara,
    string?   Assunto,
    decimal?  ValorCausa,
    decimal?  HonorariosEstimados,
    string?   FaseProcessual,
    string?   Status,
    DateOnly? DataDistribuicao,
    string?   Observacoes
);
