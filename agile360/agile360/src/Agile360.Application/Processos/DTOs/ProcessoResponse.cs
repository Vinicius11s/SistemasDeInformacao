namespace Agile360.Application.Processos.DTOs;

public record ProcessoResponse(
    Guid      Id,
    Guid      IdAdvogado,
    Guid      IdCliente,
    string    NumProcesso,
    string    Status,
    string?   ParteContraria,
    string?   Tribunal,
    string?   ComarcaVara,
    string?   Assunto,
    decimal?  ValorCausa,
    decimal?  HonorariosEstimados,
    string?   FaseProcessual,
    DateOnly? DataDistribuicao,
    string?   Observacoes,
    DateOnly? CriadoEm
);
