namespace Agile360.Application.Clientes.DTOs;

/// <summary>Resposta JSON para GET /api/clientes e GET /api/clientes/{id}</summary>
public record ClienteResponse(
    Guid      Id,
    Guid      IdAdvogado,
    string    TipoCliente,
    string    NomeCompleto,
    string?   Cpf,
    string?   Rg,
    string?   OrgaoExpedidor,
    DateOnly? DataNascimento,
    string?   EstadoCivil,
    string?   Profissao,
    string?   Telefone,
    string?   NumeroConta,
    string?   Pix,
    string?   Cep,
    string?   Endereco,
    string?   Numero,
    string?   Bairro,
    string?   Complemento,
    string?   Cidade,
    string?   Estado,
    DateOnly? DataCadastro
);
