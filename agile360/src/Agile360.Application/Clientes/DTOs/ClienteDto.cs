namespace Agile360.Application.Clientes.DTOs;

public record ClienteResponse(
    Guid      Id,
    string    TipoCliente,
    // PF
    string?   NomeCompleto,
    string?   CPF,
    string?   RG,
    string?   OrgaoExpedidor,
    // PJ
    string?   RazaoSocial,
    string?   CNPJ,
    string?   InscricaoEstadual,
    // Contato
    string?   Telefone,
    // Endereço
    string?   CEP,
    string?   Estado,
    string?   Cidade,
    string?   Endereco,
    string?   Numero,
    string?   Bairro,
    string?   Complemento,
    // Dados adicionais
    DateOnly? DataReferencia,
    string?   EstadoCivil,
    string?   AreaAtuacao,
    string?   NumeroConta,
    string?   Pix,
    bool      IsActive,
    string?   Observacoes,
    DateOnly  DataCadastro
);

public record CreateClienteRequest(
    string    TipoCliente,
    string?   NomeCompleto,
    string?   CPF,
    string?   RG,
    string?   OrgaoExpedidor,
    string?   RazaoSocial,
    string?   CNPJ,
    string?   InscricaoEstadual,
    string?   Telefone,
    string?   CEP,
    string?   Estado,
    string?   Cidade,
    string?   Endereco,
    string?   Numero,
    string?   Bairro,
    string?   Complemento,
    DateOnly? DataReferencia,
    string?   EstadoCivil,
    string?   AreaAtuacao,
    string?   NumeroConta,
    string?   Pix,
    string?   Observacoes
);

public record UpdateClienteRequest(
    string?   TipoCliente,
    string?   NomeCompleto,
    string?   CPF,
    string?   RG,
    string?   OrgaoExpedidor,
    string?   RazaoSocial,
    string?   CNPJ,
    string?   InscricaoEstadual,
    string?   Telefone,
    string?   CEP,
    string?   Estado,
    string?   Cidade,
    string?   Endereco,
    string?   Numero,
    string?   Bairro,
    string?   Complemento,
    DateOnly? DataReferencia,
    string?   EstadoCivil,
    string?   AreaAtuacao,
    string?   NumeroConta,
    string?   Pix,
    bool?     IsActive,
    string?   Observacoes
);
