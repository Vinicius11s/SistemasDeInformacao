using Agile360.Domain.Entities;

namespace Agile360.UnitTests.Builders;

public class ProcessoBuilder
{
    private Guid   _id           = Guid.NewGuid();
    private Guid   _advogadoId;
    private Guid   _clienteId;
    private string _numProcesso  = "1234567-89.2024.8.26.0100";
    private string _status       = "Ativo";

    public ProcessoBuilder WithId(Guid id) { _id = id; return this; }
    public ProcessoBuilder WithAdvogadoId(Guid id) { _advogadoId = id; return this; }
    public ProcessoBuilder WithClienteId(Guid id) { _clienteId = id; return this; }
    public ProcessoBuilder WithNumeroProcesso(string numero) { _numProcesso = numero; return this; }
    public ProcessoBuilder WithStatus(string status) { _status = status; return this; }

    public Processo Build() => new()
    {
        Id          = _id,
        IdAdvogado  = _advogadoId,
        IdCliente   = _clienteId,
        NumProcesso = _numProcesso,
        Status      = _status
    };
}
