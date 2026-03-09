using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Agile360.Infrastructure.Data;

/// <summary>
/// Used by EF Core design-time tools (e.g. dotnet ef migrations add) when no request context exists.
/// </summary>
public class Agile360DbContextFactory : IDesignTimeDbContextFactory<Agile360DbContext>
{
    public Agile360DbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Agile360.API");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Supabase")
            ?? "Host=localhost;Port=5432;Database=agile360;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<Agile360DbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new Agile360DbContext(optionsBuilder.Options, tenantProvider: null);
    }
}
