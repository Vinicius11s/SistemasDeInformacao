namespace Agile360.Application.StagingPrazos.DTOs;

/// <summary>Response DTO para um registro de staging de prazo enviado ao dashboard.</summary>
public record StagingPrazoResponse(
    Guid Id,
    Guid? ProcessoId,
    Guid? ClienteId,
    string? Titulo,
    string? Descricao,
    string? TipoPrazo,
    string? Prioridade,
    DateOnly? DataVencimento,
    DateOnly? DataPublicacao,
    string? TipoContagem,
    int? PrazoDias,
    bool SuspensaoPrazos,
    string Status,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Request DTO usado pelo bot n8n para submeter um prazo pendente.
/// </summary>
public record CreateStagingPrazoRequest(
    Guid? ProcessoId,
    Guid? ClienteId,
    string? Titulo,
    string? Descricao,
    string? TipoPrazo,
    string? Prioridade,
    DateOnly? DataVencimento,
    DateOnly? DataPublicacao,
    string? TipoContagem,
    int? PrazoDias,
    bool SuspensaoPrazos,
    string? OrigemMensagem
);

/// <summary>
/// Request DTO usada pelo advogado para editar antes da ativação.
/// Todos os campos são opcionais; valores nulos não alteram o staging.
/// </summary>
public record UpdateStagingPrazoRequest(
    string? Titulo,
    DateOnly? DataVencimento,
    string? Prioridade,
    string? TipoContagem
);

public record ConfirmarStagingPrazoRequest(Guid? IdCliente, Guid? IdProcesso);

/// <summary>Resumo leve para badge / card do dashboard.</summary>
public record StagingPrazoCountResponse(int Pendentes);

