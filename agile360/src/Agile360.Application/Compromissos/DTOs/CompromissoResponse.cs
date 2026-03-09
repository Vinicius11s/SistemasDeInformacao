namespace Agile360.Application.Compromissos.DTOs;

public record CompromissoResponse(
    Guid            Id,
    Guid            AdvogadoId,
    string          TipoCompromisso,
    string?         TipoAudiencia,
    bool            IsActive,
    DateOnly        Data,
    TimeOnly        Hora,
    string?         Local,
    Guid?           ClienteId,
    Guid?           ProcessoId,
    string?         Observacoes,
    int?            LembreteMinutos,
    DateTimeOffset  CreatedAt
);
