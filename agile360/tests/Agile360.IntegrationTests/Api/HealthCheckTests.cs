using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Agile360.IntegrationTests.Api;

public class HealthCheckTests : IClassFixture<Agile360WebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(Agile360WebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ApiHealth_Returns200AndHealthy()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<HealthResponse>();
        json.Should().NotBeNull();
        json!.Data.Should().NotBeNull();
        json.Data!.Status.Should().Be("Healthy");
    }

    private sealed class HealthResponse
    {
        public bool Success { get; set; }
        public HealthData? Data { get; set; }
    }

    private sealed class HealthData
    {
        public string Status { get; set; } = "";
        public string Version { get; set; } = "";
    }
}
