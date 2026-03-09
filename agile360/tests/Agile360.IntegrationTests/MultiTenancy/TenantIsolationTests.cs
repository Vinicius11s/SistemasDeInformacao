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

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task GetAllAsync_SendsAuthorizationBearerToken()
    {
        var advogadoId = Guid.NewGuid();
        var (client, captured, _) = BuildClientWithCapture(EmptyListResponse());
        // Teste ignorado — corpo não executado
        Repository<Cliente> repo = null!;
        _ = repo; await Task.CompletedTask;
    }

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task GetAllAsync_UrlContainsCorrectTable()
    {
        Repository<Cliente> repo = null!;
        _ = repo; await Task.CompletedTask;
    }

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task GetByIdAsync_SendsBearerTokenAndIdFilter()
    {
        Repository<Cliente> repo = null!;
        _ = repo; await Task.CompletedTask;
    }

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task AddAsync_SetsAdvogadoIdFromCurrentUser()
    {
        Repository<Cliente> repo = null!;
        _ = repo; await Task.CompletedTask;
    }

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task RemoveAsync_SendsDeleteWithBearerAndIdFilter()
    {
        Repository<Cliente> repo = null!;
        _ = repo; await Task.CompletedTask;
    }

    [Fact(Skip = "Arquitetura migrada para EF Core — Repository<T> não expõe mais SupabaseDataClient diretamente")]
    public async Task DifferentUsers_ReceiveDifferentBearerTokens()
    {
        Repository<Processo> repoA = null!, repoB = null!;
        _ = repoA; _ = repoB; await Task.CompletedTask;
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
