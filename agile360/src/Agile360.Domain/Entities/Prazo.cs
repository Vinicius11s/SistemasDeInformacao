using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

public class Prazo : BaseEntity
{
    public Guid ProcessoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTimeOffset DataVencimento { get; set; }
    public TipoPrazo Tipo { get; set; }
    public PrioridadePrazo Prioridade { get; set; }
    public StatusPrazo Status { get; set; }
    public bool AlertaEnviado { get; set; }
    public string? OrigemIntimacao { get; set; }
}
