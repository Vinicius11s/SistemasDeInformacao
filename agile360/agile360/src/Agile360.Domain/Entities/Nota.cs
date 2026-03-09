namespace Agile360.Domain.Entities;

public class Nota : BaseEntity
{
    public Guid? ProcessoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool Fixada { get; set; }
}
