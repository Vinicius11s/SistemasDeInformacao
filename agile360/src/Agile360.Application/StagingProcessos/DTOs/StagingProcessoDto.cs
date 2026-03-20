namespace Agile360.Application.StagingProcessos.DTOs;

/// <summary>
/// Response DTO para um registro de staging de processo enviado ao dashboard para revisão.
/// </summary>
public record StagingProcessoResponse(
    Guid            Id,
    string?         NumProcesso,
    string?         ParteContraria,
    string?         Tribunal,
    string?         ComarcaVara,
    string?         Assunto,
    decimal?        ValorCausa,
    decimal?        HonorariosEstimados,
    string?         FaseProcessual,
    string?         StatusProcesso,
    DateOnly?       DataDistribuicao,
    string?         ClienteNome,
    string?         Observacoes,
    string?         OrigemMensagem,
    string          Status,
    DateTimeOffset  ExpiresAt,
    DateTimeOffset  CreatedAt
);

/// <summary>
/// Request DTO usado pelo bot n8n para submeter um processo pendente.
/// Todos os campos são opcionais — o bot pode coletar dados parciais.
/// </summary>
public record CreateStagingProcessoRequest(
    string?   NumProcesso,
    string?   ParteContraria,
    string?   Tribunal,
    string?   ComarcaVara,
    string?   Assunto,
    decimal?  ValorCausa,
    decimal?  HonorariosEstimados,
    string?   FaseProcessual,
    string?   StatusProcesso,
    DateOnly? DataDistribuicao,
    string?   ClienteNome,   // Nome do cliente — o bot pode não conhecer o Guid
    string?   Observacoes,
    string?   OrigemMensagem // Texto bruto da mensagem WhatsApp — contexto para revisão
);

/// <summary>
/// Request DTO usada pelo advogado para editar antes da ativação.
/// Todos os campos são opcionais; valores nulos não alteram o staging.
/// </summary>
public record UpdateStagingProcessoRequest(
    string?  NumProcesso,
    string?  ParteContraria,
    decimal? ValorCausa,
    string?  Tribunal,
    string?  ComarcaVara,
    string?  Assunto
);

/// <summary>Resumo leve para badge / card do dashboard.</summary>
public record StagingProcessoCountResponse(int Pendentes);
