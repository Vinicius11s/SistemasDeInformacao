namespace Agile360.Application.Clientes.DTOs;

/// <summary>
/// Representa uma linha já extraída do Excel pelo Controller.
/// O Service recebe esta lista e aplica as regras de negócio:
/// validação de CPF, detecção de duplicados e persistência em lote.
/// </summary>
public record ImportarClienteRow(
    int     Linha,
    string  NomeCompleto,
    string? Cpf,
    string? Rg,
    string? OrgaoExpedidor,
    string? DataNascimentoRaw,   // dd/MM/yyyy — parseado pelo service
    string? EstadoCivil,
    string? Profissao,
    string? Telefone,
    string? NumeroConta,
    string? Pix,
    string? Cep,
    string? Endereco,
    string? Numero,
    string? Bairro,
    string? Complemento,
    string? Cidade,
    string? Estado               // max 2 chars
);
