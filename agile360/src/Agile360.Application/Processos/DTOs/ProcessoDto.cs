namespace Agile360.Application.Processos.DTOs;

public record ProcessoResponse(
    Guid      Id,
    Guid?     IdCliente,          // snake_case: id_cliente  (alinhado com o frontend)
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
    Guid?     IdCliente,          // snake_case: id_cliente  (alinhado com o frontend)
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
    Guid?     IdCliente,          // snake_case: id_cliente  (alinhado com o frontend)
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
