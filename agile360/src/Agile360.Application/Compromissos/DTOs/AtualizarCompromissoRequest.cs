namespace Agile360.Application.Compromissos.DTOs;

public record AtualizarCompromissoRequest(
    string    TipoCompromisso,
    string?   TipoAudiencia,
    bool?     IsActive,   // null = não altera
    DateOnly  Data,
    TimeOnly  Hora,
    string?   Local,
    Guid?     IdCliente,
    Guid?     IdProcesso,
    string?   Observacoes,
    int?      LembreteMinutos
);
