using Agile360.Domain.Entities;
using Agile360.Domain.Enums;

namespace Agile360.UnitTests.Builders;

public class ClienteBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _advogadoId;
    private string _nome = "Cliente Teste";
    private string? _cpf;
    private OrigemCliente _origem = OrigemCliente.Manual;
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _updatedAt = DateTimeOffset.UtcNow;

    public ClienteBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ClienteBuilder WithAdvogadoId(Guid advogadoId)
    {
        _advogadoId = advogadoId;
        return this;
    }

    public ClienteBuilder WithNome(string nome)
    {
        _nome = nome;
        return this;
    }

    public ClienteBuilder WithCpf(string cpf)
    {
        _cpf = cpf;
        return this;
    }

    public Cliente Build() => new()
    {
        Id = _id,
        AdvogadoId = _advogadoId,
        Nome = _nome,
        CPF = _cpf,
        Origem = _origem,
        IsActive = _isActive,
        CreatedAt = _createdAt,
        UpdatedAt = _updatedAt
    };
}
