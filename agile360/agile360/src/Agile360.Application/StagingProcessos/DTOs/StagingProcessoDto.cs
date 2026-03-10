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
    /// <summary>Nome do cliente — o bot pode não conhecer o Guid.</summary>
    string?   ClienteNome,
    string?   Observacoes,
    /// <summary>Texto bruto da mensagem WhatsApp — contexto para revisão.</summary>
    string?   OrigemMensagem
);

/// <summary>Resumo leve para badge / card do dashboard.</summary>
public record StagingProcessoCountResponse(int Pendentes);
