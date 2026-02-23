using Agile360.Domain.Entities;

namespace Agile360.UnitTests.Builders;

public class AdvogadoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _nome = "Advogado Teste";
    private string _email = "advogado@teste.com";
    private string _oab = "OAB/SP 123456";
    private bool _ativo = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _updatedAt = DateTimeOffset.UtcNow;

    public AdvogadoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AdvogadoBuilder WithNome(string nome)
    {
        _nome = nome;
        return this;
    }

    public AdvogadoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public Advogado Build() => new()
    {
        Id = _id,
        Nome = _nome,
        Email = _email,
        OAB = _oab,
        Ativo = _ativo,
        CreatedAt = _createdAt,
        UpdatedAt = _updatedAt
    };
}
