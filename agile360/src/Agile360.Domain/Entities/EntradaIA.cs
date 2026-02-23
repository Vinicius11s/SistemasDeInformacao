using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

public class EntradaIA : BaseEntity
{
    public OrigemEntradaIA Origem { get; set; }
    public string ConteudoOriginal { get; set; } = string.Empty;
    public string? DadosExtraidos { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProcessoId { get; set; }
    public StatusEntradaIA Status { get; set; }
    public DateTime? ProcessadoEm { get; set; }
}
