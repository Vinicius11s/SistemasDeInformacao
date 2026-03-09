namespace Agile360.Application.Processos.DTOs;

/// <summary>Payload para POST /api/processos</summary>
public record CriarProcessoRequest(
    Guid     IdCliente,
    string   NumProcesso,
    string   Status,              // Ativo | Suspenso | Arquivado | Encerrado
    string?  ParteContraria,
    string?  Tribunal,
    string?  ComarcaVara,
    string?  Assunto,
    decimal? ValorCausa,
    decimal? HonorariosEstimados,
    string?  FaseProcessual,      // Conhecimento | Recursal | Execução
    DateOnly? DataDistribuicao,
    string?  Observacoes
);
