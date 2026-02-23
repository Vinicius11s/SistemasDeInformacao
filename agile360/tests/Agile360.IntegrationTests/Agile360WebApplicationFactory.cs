using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Agile360.IntegrationTests;

/// <summary>
/// WebApplicationFactory for integration tests. Uses test configuration (e.g. in-memory or test DB).
/// </summary>
public class Agile360WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Supabase"] = "Host=localhost;Port=5432;Database=agile360_test;Username=postgres;Password=postgres;SSL Mode=Disable",
                ["JwtSettings:Secret"] = "test-secret",
                ["JwtSettings:Issuer"] = "https://test",
                ["JwtSettings:Audience"] = "test"
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }
}
