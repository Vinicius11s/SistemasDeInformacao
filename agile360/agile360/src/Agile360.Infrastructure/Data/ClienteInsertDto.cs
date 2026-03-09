namespace Agile360.Infrastructure.Data;

/// <summary>
/// DTO de inserção para a tabela 'cliente' no Supabase PostgREST.
///
/// Por que este DTO existe?
///   O PostgREST rejeita um batch insert onde os objetos possuem chaves
///   diferentes (PGRST102: "All object keys must match").  Isso acontece
///   porque o <see cref="SupabaseDataClient.JsonOpts"/> usa
///   <c>WhenWritingNull</c>, que OMITE campos nulos do JSON — fazendo
///   registros com telefone/complemento/etc. vazios terem menos chaves
///   que registros completos.
///
///   Este DTO resolve os dois problemas:
///     1. É serializado com <see cref="SupabaseDataClient.JsonOptsBatchInsert"/>
///        (sem WhenWritingNull), garantindo que TODOS os campos apareçam
///        no JSON, inclusive os nulos.
///     2. Omite <c>data_cadastro</c> intencionalmente, deixando o banco
///        usar o valor DEFAULT now() em vez de receber null.
/// </summary>
internal sealed record ClienteInsertDto(
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
    string?   Estado
    // data_cadastro é OMITIDO aqui — o Supabase usa DEFAULT now()
    // Incluir como null sobrescreveria o DEFAULT com NULL no banco
);
