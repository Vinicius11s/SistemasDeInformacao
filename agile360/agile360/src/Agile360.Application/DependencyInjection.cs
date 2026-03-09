using System.Reflection;
using Agile360.Application.Behaviors;
using Agile360.Application.Clientes.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agile360.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // ── Serviços de aplicação ────────────────────────────────────────────
        services.AddScoped<IClienteBulkImportService, ClienteBulkImportService>();

        return services;
    }
}
