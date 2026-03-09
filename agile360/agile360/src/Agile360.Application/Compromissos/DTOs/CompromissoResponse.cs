namespace Agile360.Application.Compromissos.DTOs;

public record CompromissoResponse(
    Guid      Id,
    Guid      IdAdvogado,
    string    TipoCompromisso,
    string?   TipoAudiencia,
    string    Status,
    DateOnly  Data,
    TimeOnly  Hora,
    string?   Local,
    Guid?     IdCliente,
    Guid?     IdProcesso,
    string?   Observacoes,
    int?      LembreteMinutos,
    DateOnly? CriadoEm
);
