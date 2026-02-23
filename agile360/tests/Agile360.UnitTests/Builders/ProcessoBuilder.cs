using Agile360.Domain.Entities;
using Agile360.Domain.Enums;

namespace Agile360.UnitTests.Builders;

public class ProcessoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _advogadoId;
    private Guid _clienteId;
    private string _numeroProcesso = "1234567-89.2024.8.26.0100";
    private StatusProcesso _status = StatusProcesso.Ativo;
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _updatedAt = DateTimeOffset.UtcNow;

    public ProcessoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProcessoBuilder WithAdvogadoId(Guid advogadoId)
    {
        _advogadoId = advogadoId;
        return this;
    }

    public ProcessoBuilder WithClienteId(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public ProcessoBuilder WithNumeroProcesso(string numero)
    {
        _numeroProcesso = numero;
        return this;
    }

    public ProcessoBuilder WithStatus(StatusProcesso status)
    {
        _status = status;
        return this;
    }

    public Processo Build() => new()
    {
        Id = _id,
        AdvogadoId = _advogadoId,
        ClienteId = _clienteId,
        NumeroProcesso = _numeroProcesso,
        Status = _status,
        IsActive = _isActive,
        CreatedAt = _createdAt,
        UpdatedAt = _updatedAt
    };
}
