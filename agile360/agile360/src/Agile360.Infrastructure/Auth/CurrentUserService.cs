using System.Security.Claims;
using Agile360.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agile360.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public const string AdvogadoIdClaim = "advogado_id";
    public const string AdvogadoNomeClaim = "advogado_nome";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid AdvogadoId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst(AdvogadoIdClaim)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value ?? string.Empty;

    public string Nome => _httpContextAccessor.HttpContext?.User?.FindFirst(AdvogadoNomeClaim)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Token bruto do cabeçalho Authorization: Bearer &lt;token&gt;.
    /// Passado diretamente para o Supabase Data API para que o RLS seja aplicado.
    /// </summary>
    public string? AccessToken
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            return header?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? header["Bearer ".Length..]
                : null;
        }
    }
}
