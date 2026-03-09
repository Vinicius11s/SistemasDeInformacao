using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Infrastructure.Auth;
using Agile360.Infrastructure.Data;
using Agile360.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Agile360.IntegrationTests.MultiTenancy;

/// <summary>
/// Testa o isolamento multi-tenant via Supabase PostgREST:
/// verifica que cada repositório envia Authorization: Bearer com o token do usuário,
/// garantindo que o RLS do Supabase filtre os dados pelo AdvogadoId = auth.uid().
/// Usa um HttpMessageHandler mockado para interceptar as chamadas HTTP.
/// </summary>
public class TenantIsolationTests
{
    private const string FakeToken    = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ0ZXN0In0.signature";
    private const string FakeBaseUrl  = "https://test.supabase.co";
    private const string FakeAnonKey  = "anon-key-test";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = null
    };

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static (SupabaseDataClient client, List<HttpRequestMessage> captured, MockHttpHandler handler)
        BuildClientWithCapture(HttpResponseMessage response)
    {
        var captured = new List<HttpRequestMessage>();
        var handler  = new MockHttpHandler(req =>
        {
            captured.Add(req);
            return Task.FromResult(response);
        });
        var http    = new HttpClient(handler);
        var options = Options.Create(new SupabaseAuthOptions
        {
            BaseUrl        = FakeBaseUrl,
            AnonKey        = FakeAnonKey,
            ServiceRoleKey = "service-role-key"
        });
        var dataClient = new SupabaseDataClient(http, options);
        return (dataClient, captured, handler);
    }

    private static ICurrentUserService FakeUser(Guid advogadoId, string token = FakeToken)
    {
        var svc = new FakeCurrentUserService
        {
            AdvogadoId    = advogadoId,
            AccessToken   = token,
            IsAuthenticated = true
        };
        return svc;
    }

    private static HttpResponseMessage JsonResponse<T>(T payload)
    {
        var json    = JsonSerializer.Serialize(new[] { payload }, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static HttpResponseMessage EmptyListResponse()
    {
        var content = new StringContent("[]", Encoding.UTF8, "application/json");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    // ─── Testes ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_SendsAuthorizationBearerToken()
    {
        var advogadoId = Guid.NewGuid();
        var (client, captured, _) = BuildClientWithCapture(EmptyListResponse());
        var repo = new Repository<Cliente>(client, FakeUser(advogadoId));

        await repo.GetAllAsync();

        captured.Should().HaveCount(1);
        var req = captured[0];
        req.Headers.Authorization.Should().NotBeNull();
        req.Headers.Authorization!.Scheme.Should().Be("Bearer");
        req.Headers.Authorization.Parameter.Should().Be(FakeToken);
    }

    [Fact]
    public async Task GetAllAsync_UrlContainsCorrectTable()
    {
        var (client, captured, _) = BuildClientWithCapture(EmptyListResponse());
        var repo = new Repository<Cliente>(client, FakeUser(Guid.NewGuid()));

        await repo.GetAllAsync();

        captured[0].RequestUri!.AbsoluteUri.Should().Contain("/rest/v1/cliente"); // tabela singular
    }

    [Fact]
    public async Task GetByIdAsync_SendsBearerTokenAndIdFilter()
    {
        var id         = Guid.NewGuid();
        var advogadoId = Guid.NewGuid();
        var cliente    = new Cliente { Id = id, IdAdvogado = advogadoId, NomeCompleto = "Teste" };

        var (client, captured, _) = BuildClientWithCapture(JsonResponse(cliente));
        var repo = new Repository<Cliente>(client, FakeUser(advogadoId));

        var result = await repo.GetByIdAsync(id);

        captured.Should().HaveCount(1);
        captured[0].RequestUri!.Query.Should().Contain($"id=eq.{id}");   // snake_case
        captured[0].Headers.Authorization!.Parameter.Should().Be(FakeToken);
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task AddAsync_SetsAdvogadoIdFromCurrentUser()
    {
        var advogadoId = Guid.NewGuid();
        var novoCliente = new Cliente { NomeCompleto = "Novo Cliente" };

        var (client, _, handler) = BuildClientWithCapture(
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new[] { novoCliente }, JsonOpts),
                    Encoding.UTF8, "application/json")
            });

        var repo = new Repository<Cliente>(client, FakeUser(advogadoId));

        await repo.AddAsync(novoCliente);

        // O corpo enviado ao PostgREST deve conter o AdvogadoId do usuário autenticado
        handler.RequestBodies.Should().HaveCount(1);
        handler.RequestBodies[0].Should().Contain(advogadoId.ToString());
    }

    [Fact]
    public async Task RemoveAsync_SendsDeleteWithBearerAndIdFilter()
    {
        var id         = Guid.NewGuid();
        var advogadoId = Guid.NewGuid();
        var cliente    = new Cliente { Id = id, IdAdvogado = advogadoId, NomeCompleto = "Del" };

        var (client, captured, _) = BuildClientWithCapture(
            new HttpResponseMessage(HttpStatusCode.NoContent));

        var repo = new Repository<Cliente>(client, FakeUser(advogadoId));
        await repo.RemoveAsync(cliente);

        captured[0].Method.Should().Be(HttpMethod.Delete);
        captured[0].RequestUri!.Query.Should().Contain($"id=eq.{id}");   // snake_case
        captured[0].Headers.Authorization!.Parameter.Should().Be(FakeToken);
    }

    [Fact]
    public async Task DifferentUsers_ReceiveDifferentBearerTokens()
    {
        // Garante que o token enviado ao Supabase é o do usuário atual (a base do RLS)
        var tokenA = "token-usuario-A";
        var tokenB = "token-usuario-B";

        var (clientA, capturedA, _)  = BuildClientWithCapture(EmptyListResponse());
        var (clientB, capturedB, __) = BuildClientWithCapture(EmptyListResponse());

        var repoA = new Repository<Processo>(clientA, FakeUser(Guid.NewGuid(), tokenA));
        var repoB = new Repository<Processo>(clientB, FakeUser(Guid.NewGuid(), tokenB));

        await repoA.GetAllAsync();
        await repoB.GetAllAsync();

        capturedA[0].Headers.Authorization!.Parameter.Should().Be(tokenA);
        capturedB[0].Headers.Authorization!.Parameter.Should().Be(tokenB);
    }
}

// ─── Auxiliares de teste ──────────────────────────────────────────────────────

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid AdvogadoId    { get; init; }
    public string Email       { get; init; } = string.Empty;
    public string Nome        { get; init; } = string.Empty;
    public bool IsAuthenticated { get; init; }
    public string? AccessToken  { get; init; }
}

internal sealed class MockHttpHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;
    // Corpo das requisições lidos antes do objeto ser descartado
    public List<string> RequestBodies { get; } = [];

    public MockHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        => _handler = handler;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is not null)
            RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        return await _handler(request);
    }
}
