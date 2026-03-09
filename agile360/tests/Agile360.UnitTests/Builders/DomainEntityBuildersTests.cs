using Agile360.UnitTests.Builders;
using FluentAssertions;
using Xunit;

namespace Agile360.UnitTests.Domain;

public class DomainEntityBuildersTests
{
    [Fact]
    public void AdvogadoBuilder_Build_ReturnsValidAdvogado()
    {
        var id = Guid.NewGuid();
        var advogado = new AdvogadoBuilder()
            .WithId(id)
            .WithNome("Dr. Silva")
            .WithEmail("silva@oab.com")
            .Build();

        advogado.Id.Should().Be(id);
        advogado.Nome.Should().Be("Dr. Silva");
        advogado.Email.Should().Be("silva@oab.com");
    }

    [Fact]
    public void ClienteBuilder_WithAdvogadoId_BuildAssignsTenant()
    {
        var advogadoId = Guid.NewGuid();
        var cliente = new ClienteBuilder()
            .WithAdvogadoId(advogadoId)
            .WithNome("Cliente X")
            .Build();

        cliente.AdvogadoId.Should().Be(advogadoId);
        cliente.NomeCompleto.Should().Be("Cliente X");
    }

    [Fact]
    public void ProcessoBuilder_WithAdvogadoAndCliente_BuildAssignsBoth()
    {
        var advogadoId = Guid.NewGuid();
        var clienteId  = Guid.NewGuid();
        var processo   = new ProcessoBuilder()
            .WithAdvogadoId(advogadoId)
            .WithClienteId(clienteId)
            .WithNumeroProcesso("0000000-00.2024.8.26.0100")
            .Build();

        processo.AdvogadoId.Should().Be(advogadoId);
        processo.ClienteId.Should().Be(clienteId);
        processo.NumProcesso.Should().Contain("2024");
    }
}
