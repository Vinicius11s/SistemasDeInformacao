using Agile360.Application.Interfaces;
using Agile360.Domain.Interfaces;

namespace Agile360.API.MultiTenancy;

/// <summary>
/// Resolves current tenant from JWT (ICurrentUserService) when authenticated; falls back to X-Advogado-Id header for dev/test.
/// </summary>
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUser;
    private Guid? _currentAdvogadoId;

    public const string AdvogadoIdKey = "advogado_id";
    public const string AdvogadoIdHeader = "X-Advogado-Id";

    public TenantProvider(IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUser)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    public Guid GetCurrentAdvogadoId()
    {
        if (_currentAdvogadoId.HasValue)
            return _currentAdvogadoId.Value;

        if (_currentUser.IsAuthenticated && _currentUser.AdvogadoId != Guid.Empty)
        {
            SetCurrentAdvogadoId(_currentUser.AdvogadoId);
            return _currentUser.AdvogadoId;
        }

        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(AdvogadoIdKey, out var value) == true && value is Guid id)
            return id;

        if (context?.Request.Headers.TryGetValue(AdvogadoIdHeader, out var header) == true &&
            Guid.TryParse(header, out var headerId))
            return headerId;

        return Guid.Empty;
    }

    public void SetCurrentAdvogadoId(Guid advogadoId)
    {
        _currentAdvogadoId = advogadoId;
        _httpContextAccessor.HttpContext?.Items.TryAdd(AdvogadoIdKey, advogadoId);
    }
}
