using Agile360.Domain.Entities;

namespace Agile360.UnitTests.Builders;

public class AdvogadoBuilder
{
    private Guid   _id        = Guid.NewGuid();
    private string _nome      = "Advogado Teste";
    private string _email     = "advogado@teste.com";
    private string _numeroOab = "123456";
    private string _oabUf     = "SP";
    private string _status    = "ativa";
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public AdvogadoBuilder WithId(Guid id)          { _id = id;          return this; }
    public AdvogadoBuilder WithNome(string nome)     { _nome = nome;      return this; }
    public AdvogadoBuilder WithEmail(string email)   { _email = email;    return this; }
    public AdvogadoBuilder WithOab(string oab, string uf = "SP")
    {
        _numeroOab = oab;
        _oabUf = uf;
        return this;
    }
    public AdvogadoBuilder WithStatus(string status) { _status = status;  return this; }

    public Advogado Build() => new()
    {
        Id               = _id,
        Nome             = _nome,
        Email            = _email,
        NumeroOab        = _numeroOab,
        OabUf            = _oabUf,
        StatusAssinatura = _status,
        CreatedAt        = _createdAt
    };
}
