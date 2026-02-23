using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsAppNumero { get; set; }
    public string? Endereco { get; set; }
    public string? Observacoes { get; set; }
    public OrigemCliente Origem { get; set; }
}
