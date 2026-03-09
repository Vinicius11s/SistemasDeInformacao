using Agile360.Domain.Interfaces;

namespace Agile360.IntegrationTests.MultiTenancy;

/// <summary>
/// ITenantProvider for tests; allows setting current advogado id per test.
/// </summary>
public sealed class TestTenantProvider : ITenantProvider
{
    private Guid _currentAdvogadoId;

    public Guid GetCurrentAdvogadoId() => _currentAdvogadoId;

    public void SetCurrentAdvogadoId(Guid advogadoId) => _currentAdvogadoId = advogadoId;
}
