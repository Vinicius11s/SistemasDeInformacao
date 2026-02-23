namespace Agile360.Domain.Interfaces;

/// <summary>
/// Provides the current tenant (advogado) id for the request. Set by TenantMiddleware from JWT (Story 1.3).
/// </summary>
public interface ITenantProvider
{
    Guid GetCurrentAdvogadoId();
    void SetCurrentAdvogadoId(Guid advogadoId);
}
