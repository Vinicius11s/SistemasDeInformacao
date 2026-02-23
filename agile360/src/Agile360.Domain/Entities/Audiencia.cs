using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

public class Audiencia : BaseEntity
{
    public Guid ProcessoId { get; set; }
    public DateTimeOffset DataHora { get; set; }
    public string? Local { get; set; }
    public TipoAudiencia Tipo { get; set; }
    public StatusAudiencia Status { get; set; }
    public string? Observacoes { get; set; }
    public string? GoogleEventId { get; set; }
    public int LembretesEnviados { get; set; }
}
