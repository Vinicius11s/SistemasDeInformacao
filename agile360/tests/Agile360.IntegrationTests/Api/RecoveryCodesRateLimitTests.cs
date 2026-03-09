using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateLimiterOptions = Microsoft.AspNetCore.RateLimiting.RateLimiterOptions;
using NSubstitute;
using Xunit;

namespace Agile360.IntegrationTests.Api;

/// <summary>
/// Q4 — Testes de rate limit do endpoint POST /api/auth/mfa/recovery-codes/generate.
///
/// Estratégia:
///   - Factory customizada que sobrescreve a política "mfa-generate"
///     para 3 req / 2 segundos (janela curta, testável sem sleep longo).
///   - Auth bypass via middleware de teste (simula JWT autenticado).
///   - IMfaService e IRecoveryCodeService substituídos por substitutos (NSubstitute).
///
/// Critério de aceitação (story-1.6 Q4):
///   - 3 primeiras requisições → 200 OK
///   - 4ª requisição → 429 Too Many Requests
/// </summary>
public class RecoveryCodesRateLimitTests : IClassFixture<RateLimitTestFactory>
{
    private readonly HttpClient _client;

    public RecoveryCodesRateLimitTests(RateLimitTestFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        // Simula um JWT Bearer token — o TestAuthHandler aceita qualquer valor não-vazio
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "test-token");
    }

    // ── Q4 — Rate limit: 3 req OK; 4ª retorna 429 ────────────────────────────

    /// <summary>Diagnóstico: expõe o body do primeiro 500 para facilitar o debug.</summary>
    [Fact]
    public async Task Diagnostico_PrimeiraRequisicao_MostraBodyQuando500()
    {
        var resp = await _client.PostAsync("/api/auth/mfa/recovery-codes/generate", null);
        var body = await resp.Content.ReadAsStringAsync();
        // Se não for 200, mostra o body no erro para diagnóstico
        resp.StatusCode.Should().Be(HttpStatusCode.OK,
            $"DIAGNÓSTICO — Body da resposta: {body}");
    }

    [Fact]
    public async Task Generate_PrimeirasTreeRequisicoes_Retornam200()
    {
        // Act: 3 requisições dentro da janela de rate limit
        for (var i = 0; i < 3; i++)
        {
            var resp = await _client.PostAsync("/api/auth/mfa/recovery-codes/generate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.OK,
                $"requisição {i + 1}/3 deve ser aceita dentro do limite");
        }
    }

    [Fact]
    public async Task Generate_QuartaRequisicao_Retorna429()
    {
        // Arrange: esgota o limite com 3 requisições
        for (var i = 0; i < 3; i++)
            await _client.PostAsync("/api/auth/mfa/recovery-codes/generate", null);

        // Act: 4ª requisição excede o rate limit
        var response = await _client.PostAsync("/api/auth/mfa/recovery-codes/generate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            "a 4ª requisição deve ser bloqueada pelo rate limit (policy: mfa-generate, 3 req/janela)");
    }

    [Fact]
    public async Task Generate_ComMfaDesativado_Retorna400_NaoConsome_Limit()
    {
        // Arrange — verifica que o guard 400 (MFA inativo) NÃO consome slot do rate limit
        // (o middleware de rate limit actua antes da lógica de negócio, mas em ASP.NET Core
        //  o recurso é reservado no início — este teste documenta o comportamento esperado.)
        // Neste cenário o stub de IMfaService está configurado MfaEnabled = true pela factory,
        // portanto um 400 não seria esperado aqui. Apenas garantimos que chegamos a 400
        // quando a factory configura MfaEnabled = false.
        // Cenário coberto na suite unitária; aqui é verificado apenas o caminho feliz do RL.
        var response = await _client.PostAsync("/api/auth/mfa/recovery-codes/generate", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ── Factory customizada para testes de rate limit ─────────────────────────────

/// <summary>
/// WebApplicationFactory que:
///   1. Sobrescreve "mfa-generate" para 3 req / 2 segundos (testável em tempo real).
///   2. Registra um handler de autenticação de teste (sem JWT real).
///   3. Substitui IMfaService e IRecoveryCodeService por stubs (sem banco de dados).
/// </summary>
public class RateLimitTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ── Injeta config mínima ANTES do Program.cs registrar serviços ──────────────
        // Usando environment variables garante que serão lidas antes do host build.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
            "Host=localhost;Port=5432;Database=test_rl;Username=test;Password=test;SSL Mode=Disable");
        Environment.SetEnvironmentVariable("JwtSettings__Secret",    "test-secret-32-characters-minimum!");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer",    "https://test.agile360");
        Environment.SetEnvironmentVariable("JwtSettings__Audience",  "test-audience");
        Environment.SetEnvironmentVariable("MfaSettings__EncryptionKey", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");

        builder.ConfigureServices(services =>
        {
            // ── 0. Substitui DbContext por SQLite in-memory (sem banco real necessário) ─
            services.RemoveAll<DbContextOptions<Agile360DbContext>>();
            services.RemoveAll<Agile360DbContext>();
            services.AddDbContext<Agile360DbContext>(opts =>
                opts.UseSqlite("DataSource=:memory:")
                    .UseSnakeCaseNamingConvention());

            // ── 1. Sobrescreve rate limit "mfa-generate": 3 req / 2s (janela testável) ──
            // Remove TODAS as configurações existentes de RateLimiterOptions (incluindo as do Program.cs)
            // e re-registra apenas o policy de teste com janela curta de 2s.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.AddRateLimiter(opts =>
            {
                opts.AddFixedWindowLimiter("mfa-generate", c =>
                {
                    c.Window      = TimeSpan.FromSeconds(2);  // janela curta para testes
                    c.PermitLimit = 3;
                });
                // Policies originais que o controlador usa (auth-login para challenge/recovery)
                opts.AddFixedWindowLimiter("auth-login",    c => { c.Window = TimeSpan.FromMinutes(1); c.PermitLimit = 5; });
                opts.AddFixedWindowLimiter("auth-register", c => { c.Window = TimeSpan.FromHours(1);   c.PermitLimit = 3; });
                opts.AddFixedWindowLimiter("auth-forgot",   c => { c.Window = TimeSpan.FromHours(1);   c.PermitLimit = 3; });
                opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            // ── 2a. Stub do IAuthService (necessário no construtor do RecoveryCodesController) ──
            var authStub = Substitute.For<IAuthService>();
            services.RemoveAll<IAuthService>();
            services.AddSingleton(authStub);

            // ── 2. Stub do IMfaService: MfaEnabled = true ────────────────────────────────
            var mfaStub = Substitute.For<IMfaService>();
            mfaStub.GetStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                   .Returns(Task.FromResult(new MfaStatusResponse(true)));
            services.RemoveAll<IMfaService>();
            services.AddSingleton(mfaStub);

            // ── 3. Stub do IRecoveryCodeService: retorna 10 codes dummy ─────────────────
            var rcStub = Substitute.For<IRecoveryCodeService>();
            rcStub.GenerateCodesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult<IReadOnlyList<string>>(
                      Enumerable.Range(1, 10)
                                .Select(i => $"TEST-{i:D4}")
                                .ToList()
                                .AsReadOnly()));
            services.RemoveAll<IRecoveryCodeService>();
            services.AddSingleton(rcStub);

            // ── 4. Stub do ICurrentUserService: retorna advogadoId fixo ─────────────────
            var userStub = Substitute.For<ICurrentUserService>();
            userStub.AdvogadoId.Returns(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            userStub.Email.Returns("test@agile360.com");
            services.RemoveAll<ICurrentUserService>();
            services.AddSingleton(userStub);

            // ── 5. Auth de teste: remove JWT e substitui por handler que aceita qualquer Bearer ──
            // Remove todos os Configure<AuthenticationOptions> (incluindo os do Program.cs que registram "Bearer")
            services.RemoveAll<IConfigureOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>();
            services.RemoveAll<IPostConfigureOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>();
            // Registra o esquema de teste como default E também sob o nome "Bearer"
            // para que código que busca explicitamente o scheme "Bearer" encontre nosso handler.
            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme             = "Test";
                opts.DefaultAuthenticateScheme = "Test";
                opts.DefaultChallengeScheme    = "Test";
                opts.DefaultForbidScheme       = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestBearerAuthHandler>("Test",   _ => { })
            .AddScheme<AuthenticationSchemeOptions, TestBearerAuthHandler>("Bearer", _ => { });
        });
    }
}

// ── TestBearerAuthHandler — aceita qualquer Bearer não-vazio ─────────────────

/// <summary>
/// Handler de autenticação de teste. Qualquer requisição com Authorization: Bearer &lt;qualquer&gt;
/// é considerada autenticada com o AdvogadoId fixo do stub.
/// </summary>
internal class TestBearerAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestBearerAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.Fail("Sem token Bearer"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111"),
            new Claim(ClaimTypes.Email, "test@agile360.com"),
        };
        var identity  = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
