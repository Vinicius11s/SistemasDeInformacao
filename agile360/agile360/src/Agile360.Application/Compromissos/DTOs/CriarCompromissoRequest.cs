namespace Agile360.Application.Compromissos.DTOs;

/// <summary>Payload para POST /api/compromissos</summary>
public record CriarCompromissoRequest(
    string    TipoCompromisso,    // Audiência | Atendimento | Reunião | Prazo
    string?   TipoAudiencia,     // Conciliação | Instrução e Julgamento | ... (só se tipo=Audiência)
    string    Status,             // Agendado | Concluído | Cancelado
    DateOnly  Data,
    TimeOnly  Hora,
    string?   Local,
    Guid?     IdCliente,
    Guid?     IdProcesso,
    string?   Observacoes,
    int?      LembreteMinutos    // ex.: 60 = avisar 1h antes
);
