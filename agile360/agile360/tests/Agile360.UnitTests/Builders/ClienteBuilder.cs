using Agile360.Domain.Entities;

namespace Agile360.UnitTests.Builders;

public class ClienteBuilder
{
    private Guid   _id          = Guid.NewGuid();
    private Guid   _advogadoId;
    private string _nomeCompleto = "Cliente Teste";
    private string? _cpf;

    public ClienteBuilder WithId(Guid id) { _id = id; return this; }
    public ClienteBuilder WithAdvogadoId(Guid id) { _advogadoId = id; return this; }
    public ClienteBuilder WithNome(string nome) { _nomeCompleto = nome; return this; }
    public ClienteBuilder WithCpf(string cpf) { _cpf = cpf; return this; }

    public Cliente Build() => new()
    {
        Id           = _id,
        IdAdvogado   = _advogadoId,
        NomeCompleto = _nomeCompleto,
        Cpf          = _cpf
    };
}
