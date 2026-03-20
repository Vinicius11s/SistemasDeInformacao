using System.Text;
using System.Linq;
using Microsoft.IdentityModel.JsonWebTokens;
using Agile360.API.Auth;
using Agile360.API.Middleware;
using Agile360.API.MultiTenancy;
using Agile360.Application;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", "Agile360.API")
      .WriteTo.Console(
          outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});

// ─── JWT — valida tokens emitidos pelo Supabase Auth ─────────────────────────
// Configure em appsettings.Development.json (ou User Secrets):
//   JwtSettings:Secret   = JWT secret do projeto Supabase (Settings → API → JWT Secret)
//   JwtSettings:Issuer   = https://<SEU_REF>.supabase.co/auth/v1
//   JwtSettings:Audience = authenticated
//
// Valores com '<' são placeholders — não ativam a validação correspondente.
var jwtSecret   = builder.Configuration["JwtSettings:Secret"]   ?? "";
var jwtIssuer   = builder.Configuration["JwtSettings:Issuer"]   ?? "";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "authenticated";
var masterServiceKey = builder.Configuration["MasterServiceKey"] ?? "";

// Só valida se o valor for real (não placeholder)
var validateIssuer = !string.IsNullOrEmpty(jwtIssuer) && !jwtIssuer.Contains('<');
var validateKey    = !string.IsNullOrEmpty(jwtSecret)  && !jwtSecret.Contains('<');

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // ─── .NET 8: JsonWebTokenHandler não remapeia "sub" → NameIdentifier ────
        // MapInboundClaims = false garante comportamento explícito e consistente:
        // todos os claims JWT chegam com seus nomes originais (sub, email, etc.).
        // CurrentUserService.AdvogadoId tenta "advogado_id", NameIdentifier E "sub".
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = validateIssuer,
            ValidIssuer              = jwtIssuer,
            ValidateAudience         = true,
            ValidAudience            = jwtAudience,
            ValidateIssuerSigningKey = validateKey,
            IssuerSigningKey         = validateKey
                                           ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                                           : null,
            RequireSignedTokens      = validateKey,
            ValidateLifetime         = true,

            // ─── Clock Skew — tolerância de 30s para diferenças de relógio ────
            // TimeSpan.Zero é severo demais: um servidor 1s atrasado já invalida
            // tokens recém-emitidos. 30s é o padrão de mercado para JWT.
            ClockSkew = TimeSpan.FromSeconds(30),

            // ─── Bypass de assinatura quando Secret não está configurado ─────
            // Deve retornar JsonWebToken (não JwtSecurityToken) para o novo handler.
            // PRODUÇÃO: configure JwtSettings:Secret (Supabase → Settings → API → JWT Secret).
            SignatureValidator = validateKey
                ? null
                : (token, _) => new JsonWebTokenHandler().ReadJsonWebToken(token),
        };

        // ─── Logging de autenticação (diagnóstico) ────────────────────────────
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Log.Warning("[JWT] Token rejeitado: {Reason}", ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                // Com MapInboundClaims = false, "sub" permanece como "sub".
                // Fallback para NameIdentifier preserva compatibilidade retroativa.
                var advogadoId = ctx.Principal?.FindFirst("sub")?.Value
                              ?? ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? "(sem sub)";
                Log.Debug("[JWT] Token válido — advogadoId: {AdvogadoId}", advogadoId);
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.AuthenticationScheme, _ => { });

// MasterServiceKey: Hub Central / n8n (usa X-On-Behalf-Of para selecionar o tenant)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<MasterServiceKeyAuthenticationOptions, MasterServiceKeyAuthenticationHandler>(
        MasterServiceKeyAuthenticationDefaults.AuthenticationScheme,
        options => { options.ExpectedMasterKey = masterServiceKey; });

builder.Services.AddAuthorization(options =>
{
    // Política padrão: apenas JWT.
    // Separamos ApiKey para evitar que o ApiKeyHandler tente autenticar TODOS os
    // requests JWT e emita "was not authenticated" em cada um (poluição de logs).
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    // Política explícita para endpoints que aceitam JWT *ou* API Key
    // (ex.: webhooks, integrações server-to-server, Hub Central n8n).
    // Uso: [Authorize(Policy = "JwtOrApiKey")]
    options.AddPolicy("JwtOrApiKey",
        new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                MasterServiceKeyAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build());
});

// ─── Rate limiting ────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth-login",    c => { c.Window = TimeSpan.FromMinutes(1); c.PermitLimit = 5; });
    options.AddFixedWindowLimiter("auth-register", c => { c.Window = TimeSpan.FromHours(1);   c.PermitLimit = 3; });
    options.AddFixedWindowLimiter("auth-forgot",   c => { c.Window = TimeSpan.FromHours(1);   c.PermitLimit = 3; });
    // Recovery Codes — geração envolve 10 × BCrypt(cost 12) ≈ 2.5s de CPU.
    // 3 req/hora por usuário autenticado previne DoS de CPU via endpoint /generate.
    options.AddFixedWindowLimiter("mfa-generate",  c => { c.Window = TimeSpan.FromHours(1);   c.PermitLimit = 3; });
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());

    // Para integrações server-to-server (Postman/n8n) que podem não enviar `Origin`.
    // Mesmo sem Origin, isso não impede autenticação; só garante que preflight/CORS
    // não vire um bloqueio silencioso quando Origin existir.
    options.AddPolicy("ApiIntegration", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ─── Controllers + JSON ───────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // snake_case alinha com o frontend (nome_completo, access_token, etc.)
        // e com o padrão do Supabase PostgREST.
        options.JsonSerializerOptions.PropertyNamingPolicy        = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "Agile360 API",
        Version     = "v1",
        Description = "Agile360 – CRM Jurídico API"
    });

    // ─── Suporte a API Key (X-Api-Key) no Swagger UI ─────────────────────────────
    var apiKeyScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name   = ApiKeyAuthenticationDefaults.HeaderName,
        In     = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type   = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = ApiKeyAuthenticationDefaults.AuthenticationScheme,
        Description = "Chave de integração Agile360. Envie o valor bruto gerado em Configurações → Integrações.\n\nHeader: X-Api-Key: <sua-chave>",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id   = ApiKeyAuthenticationDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(ApiKeyAuthenticationDefaults.AuthenticationScheme, apiKeyScheme);

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [apiKeyScheme] = Array.Empty<string>()
    });
});

// ─── Webhook / Tenant ─────────────────────────────────────────────────────────
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// ─── Application & Infrastructure ─────────────────────────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions       = "DENY";
    if (app.Environment.IsProduction())
        context.Response.Headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";
    await next();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<WebhookAuthMiddleware>();

// CORS deve vir antes de UseAuthentication para que os headers de CORS
// estejam presentes mesmo em respostas de erro (401, 403, 429).
app.UseCors();

// ─── [DIAG] Log de headers brutos — remover após estabilizar autenticação ──────
// Permite comparar exatamente o que o Swagger envia vs o que Postman/n8n enviam.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api/clientes") ||
        ctx.Request.Path.StartsWithSegments("/api/staging") ||
        ctx.Request.Path.StartsWithSegments("/api/compromissos") ||
        ctx.Request.Path.StartsWithSegments("/api/processos"))
    {
        var log = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        static string Trunc(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > 24 ? s[..24] + "…" : s;
        }

        // Mantém o debug de headers para comparar requests,
        // mas mascara valores sensíveis (X-Api-Key) para não vazar chaves reais.
        var headers = string.Join(" | ",
            ctx.Request.Headers.Select(h =>
            {
                var isApiKeyHeader =
                    h.Key.Equals(ApiKeyAuthenticationDefaults.HeaderName, StringComparison.OrdinalIgnoreCase);
                var isMasterKeyHeader =
                    h.Key.Equals(MasterServiceKeyAuthenticationDefaults.MasterKeyHeaderName, StringComparison.OrdinalIgnoreCase);

                var maskedValues = h.Value.Select(v =>
                {
                    var trimmed = v?.Trim() ?? "";
                    if (!isApiKeyHeader && !isMasterKeyHeader)
                        return Trunc(trimmed);

                    if (string.IsNullOrEmpty(trimmed))
                        return "";

                    // Ex: ABCD****
                    var prefix = trimmed.Length >= 4 ? trimmed[..4] : trimmed;
                    return prefix + "****";
                });

                return $"{h.Key}=[{string.Join(",", maskedValues)}]";
            }));

        log.LogDebug("[DIAG-HEADERS] {Method} {Path} → {Headers}",
            ctx.Request.Method, ctx.Request.Path, headers);
    }

    await next();
});

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();
app.UseSerilogRequestLogging();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Agile360 API v1");
        o.ConfigObject.PersistAuthorization = true;
        o.DisplayRequestDuration();
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting Agile360 API");
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Required for integration tests (WebApplicationFactory)
public partial class Program { }
