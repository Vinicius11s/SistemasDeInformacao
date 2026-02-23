using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

public class Processo : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string NumeroProcesso { get; set; } = string.Empty;
    public string? Vara { get; set; }
    public string? Comarca { get; set; }
    public string? Tribunal { get; set; }
    public string? TipoAcao { get; set; }
    public decimal? ValorCausa { get; set; }
    public StatusProcesso Status { get; set; }
    public string? Descricao { get; set; }
    public DateOnly? DataDistribuicao { get; set; }
    public DateTime? UltimaMovimentacao { get; set; }
}
