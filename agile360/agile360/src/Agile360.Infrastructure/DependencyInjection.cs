using Agile360.Application.Interfaces;
using Agile360.Application.Integration;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Auth;
using Agile360.Infrastructure.Data;
using Agile360.Infrastructure.Integration;
using Agile360.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Agile360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ─── Supabase Auth (GoTrue) ─────────────────────────────────────────────
        services.Configure<SupabaseAuthOptions>(configuration.GetSection(SupabaseAuthOptions.SectionName));
        services.AddHttpClient<SupabaseAuthClient>();

        // ─── Supabase Data API (PostgREST) ──────────────────────────────────────
        // O construtor de SupabaseDataClient já configura BaseAddress e o header
        // apikey via IOptions<SupabaseAuthOptions>. Não duplicar aqui.
        services.AddHttpClient<SupabaseDataClient>();

        // ─── Auth & Identidade ──────────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ─── Webhook ────────────────────────────────────────────────────────────
        services.AddSingleton<IWebhookSignatureValidator, WebhookSignatureValidator>();

        // ─── n8n / AI Gateway ───────────────────────────────────────────────────
        services.Configure<N8nOptions>(configuration.GetSection(N8nOptions.SectionName));
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

        // ─── Repositórios (PostgREST) ───────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IClienteRepository,     ClienteRepository>();
        services.AddScoped<IProcessoRepository,    ProcessoRepository>();
        services.AddScoped<ICompromissoRepository, CompromissoRepository>();
        services.AddScoped<IPrazoRepository,       PrazoRepository>();

        // ─── Health Check ────────────────────────────────────────────────────────
        // Verificação básica de liveness; adicione AspNetCore.HealthChecks.Uris
        // se quiser um ping HTTP dedicado ao Supabase Data API.
        services.AddHealthChecks();

        return services;
    }

    // ─── Polly ───────────────────────────────────────────────────────────────────

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(2, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
