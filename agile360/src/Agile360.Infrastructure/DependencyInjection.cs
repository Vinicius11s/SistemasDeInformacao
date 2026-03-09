using Agile360.Application.Interfaces;
using Agile360.Application.Integration;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Auth;
using Agile360.Infrastructure.Data;
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
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ─── EF Core + PostgreSQL ───────────────────────────────────────────────────
        // Lê "DefaultConnection" (User Secrets) ou "Supabase" como fallback,
        // garantindo compatibilidade com qualquer nome que tenha sido configurado.
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("Supabase")
            ?? throw new InvalidOperationException(
                "Connection string não encontrada. Configure 'DefaultConnection' via User Secrets:\n" +
                "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=db.xxx.supabase.co;...\"");

        services.AddDbContext<Agile360DbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention(); // Converte PascalCase → snake_case globalmente (ex: TipoCompromisso → tipo_compromisso)
        });

        // ─── Supabase Auth (GoTrue) ─────────────────────────────────────────────────
        services.Configure<SupabaseAuthOptions>(configuration.GetSection(SupabaseAuthOptions.SectionName));
        services.AddHttpClient<SupabaseAuthClient>();

        // ─── Supabase Data API (PostgREST — usado somente por AuthService) ──────────
        services.AddHttpClient<SupabaseDataClient>();

        // ─── Auth & Identidade ───────────────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // MFA: obrigatório apenas se MfaSettings:EncryptionKey estiver definido; caso contrário usa stub para não quebrar a API.
        var mfaEncryptionKey = configuration["MfaSettings:EncryptionKey"];
        if (!string.IsNullOrWhiteSpace(mfaEncryptionKey))
            services.AddScoped<IMfaService, MfaService>();
        else
            services.AddScoped<IMfaService, DisabledMfaService>();

        // Recovery Codes: sempre disponível, independente do MFA estar configurado.
        // O RecoveryCodesController valida MfaEnabled antes de gerar — não depende da chave de criptografia.
        services.AddScoped<IRecoveryCodeService, RecoveryCodeService>();

        // ─── Webhook ─────────────────────────────────────────────────────────────────
        services.AddSingleton<IWebhookSignatureValidator, WebhookSignatureValidator>();

        // ─── n8n / AI Gateway ────────────────────────────────────────────────────────
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

        // ─── Repositórios (EF Core) ──────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IClienteRepository,     ClienteRepository>();
        services.AddScoped<IProcessoRepository,    ProcessoRepository>();
        services.AddScoped<ICompromissoRepository, CompromissoRepository>();
        services.AddScoped<IPrazoRepository,       PrazoRepository>();
        services.AddScoped<IApiKeyRepository,      ApiKeyRepository>();

        // ─── Health Check ─────────────────────────────────────────────────────────────
        services.AddHealthChecks();

        return services;
    }

    // ─── Polly ────────────────────────────────────────────────────────────────────────

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
