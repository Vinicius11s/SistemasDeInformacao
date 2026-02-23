using Agile360.Domain.Interfaces;

namespace Agile360.API.Middleware;

/// <summary>
/// Sets current tenant (advogado_id) for the request. In Story 1.3 this will read from JWT claims.
/// For now supports header X-Advogado-Id for development/testing.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (context.Request.Headers.TryGetValue("X-Advogado-Id", out var header) &&
            Guid.TryParse(header, out var advogadoId))
        {
            tenantProvider.SetCurrentAdvogadoId(advogadoId);
        }

        await _next(context);
    }
}
