namespace Agile360.Application.Clientes.DTOs;

/// <summary>Payload para PUT /api/clientes/{id} — mesmos campos de criar</summary>
public record AtualizarClienteRequest(
    string  TipoCliente,      // 'Pessoa Física' | 'Pessoa Jurídica'
    string NomeCompleto,
    string? Cpf,
    string? Rg,
    string? OrgaoExpedidor,
    DateOnly? DataNascimento,
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
    string? Estado
);
