using Agile360.Application.Interfaces;
using Agile360.Application.Integration;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Auth;
using Agile360.Infrastructure.Data;
using Agile360.Infrastructure.Data.Interceptors;
using Agile360.Infrastructure.Integration;
using Agile360.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Agile360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SupabaseAuthOptions>(configuration.GetSection(SupabaseAuthOptions.SectionName));
        services.AddHttpClient<SupabaseAuthClient>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        // Validator is stateless and used by middleware which is constructed from the root provider.
        // Register as singleton so it can be resolved when building the middleware pipeline.
        services.AddSingleton<IWebhookSignatureValidator, WebhookSignatureValidator>();
        services.Configure<N8nOptions>(configuration.GetSection(N8nOptions.SectionName));

        // Named HttpClient for AI / n8n (Story 1.4): retry, circuit breaker, timeout
        services.AddHttpClient("Agile360.AI", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<N8nOptions>>().Value;
            client.BaseAddress = string.IsNullOrEmpty(options.BaseUrl)
                ? new Uri("http://localhost:5678")
                : new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Agile360-Backend/1.0");
            if (!string.IsNullOrEmpty(options.ApiKey))
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-N8n-Api-Key", options.ApiKey);
        })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IAiGatewayService, N8nAiGatewayService>();

        var connectionString = configuration.GetConnectionString("Supabase")
            ?? throw new InvalidOperationException("Connection string 'Supabase' not found.");

        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<Agile360DbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(2);
                npgsql.CommandTimeout(30);
            });
            options.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProcessoRepository, ProcessoRepository>();
        services.AddScoped<IAudienciaRepository, AudienciaRepository>();
        services.AddScoped<IPrazoRepository, PrazoRepository>();

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres");

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(2, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
