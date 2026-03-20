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
    string?   ClienteNome,       // Nome do cliente — o bot pode não conhecer o Guid
    string?   NumProcesso,       // Número do processo — referência textual
    string?   Observacoes,
    int?      LembreteMinutos,
    string?   OrigemMensagem     // Texto bruto da mensagem WhatsApp — contexto para revisão
);

/// <summary>
/// Request DTO usada pelo advogado para editar antes da ativação.
/// Todos os campos são opcionais; valores nulos não alteram o staging.
/// </summary>
public record UpdateStagingCompromissoRequest(
    string?   TipoCompromisso,
    DateOnly? Data,
    TimeOnly? Hora,
    string?   Local,
    int?      LembreteMinutos
);

/// <summary>Resumo leve para badge / card do dashboard.</summary>
public record StagingCompromissoCountResponse(int Pendentes);
