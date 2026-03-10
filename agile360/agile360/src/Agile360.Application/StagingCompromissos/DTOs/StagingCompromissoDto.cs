namespace Agile360.Application.StagingCompromissos.DTOs;

/// <summary>
/// Response DTO para um registro de staging de compromisso enviado ao dashboard para revisão.
/// </summary>
public record StagingCompromissoResponse(
    Guid            Id,
    string?         TipoCompromisso,
    string?         TipoAudiencia,
    DateOnly?       Data,
    TimeOnly?       Hora,
    string?         Local,
    string?         ClienteNome,
    string?         NumProcesso,
    string?         Observacoes,
    int?            LembreteMinutos,
    string?         OrigemMensagem,
    string          Status,
    DateTimeOffset  ExpiresAt,
    DateTimeOffset  CreatedAt
);

/// <summary>
/// Request DTO usado pelo bot n8n para submeter um compromisso pendente.
/// Todos os campos são opcionais — o bot pode coletar dados parciais.
/// </summary>
public record CreateStagingCompromissoRequest(
    string?   TipoCompromisso,
    string?   TipoAudiencia,
    DateOnly? Data,
    TimeOnly? Hora,
    string?   Local,
    /// <summary>Nome do cliente — o bot pode não conhecer o Guid.</summary>
    string?   ClienteNome,
    /// <summary>Número do processo — referência textual.</summary>
    string?   NumProcesso,
    string?   Observacoes,
    int?      LembreteMinutos,
    /// <summary>Texto bruto da mensagem WhatsApp — contexto para revisão.</summary>
    string?   OrigemMensagem
);

/// <summary>Resumo leve para badge / card do dashboard.</summary>
public record StagingCompromissoCountResponse(int Pendentes);
