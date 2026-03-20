using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Agile360.API.Controllers;

/// <summary>
/// Diagnóstico de conectividade com Postgres (Supabase).
/// Ajuda a confirmar host/porta/pool e medir tempo real de conexão/consulta.
/// </summary>
[Authorize]
[ApiController]
[Route("api/diag/db")]
public class DbDiagController(IConfiguration configuration, ILogger<DbDiagController> logger) : ControllerBase
{
    [HttpGet("config")]
    public IActionResult Config()
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
                 ?? configuration.GetConnectionString("Supabase")
                 ?? "";

        if (string.IsNullOrWhiteSpace(cs))
            return Ok(new { ok = false, error = "ConnectionStrings:DefaultConnection/Supabase não configurada." });

        var b = new NpgsqlConnectionStringBuilder(cs);

        return Ok(new
        {
            ok = true,
            host = b.Host,
            port = b.Port,
            database = b.Database,
            username = b.Username,
            ssl_mode = b.SslMode.ToString(),
            pooling = b.Pooling,
            max_pool_size = b.MaxPoolSize,
            min_pool_size = b.MinPoolSize,
            timeout_seconds = b.Timeout,
            command_timeout_seconds = b.CommandTimeout,
        });
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping(CancellationToken ct)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
                 ?? configuration.GetConnectionString("Supabase")
                 ?? "";

        if (string.IsNullOrWhiteSpace(cs))
            return Ok(new { ok = false, error = "ConnectionStrings:DefaultConnection/Supabase não configurada." });

        var sw = Stopwatch.StartNew();
        try
        {
            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync(ct);

            await using var cmd = new NpgsqlCommand(
                "select inet_server_addr()::text as server_ip, inet_server_port() as server_port, current_database() as db, current_user as usr;",
                conn);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);

            sw.Stop();

            return Ok(new
            {
                ok = true,
                elapsed_ms = sw.ElapsedMilliseconds,
                server_ip = reader.GetString(0),
                server_port = reader.GetInt32(1),
                database = reader.GetString(2),
                current_user = reader.GetString(3),
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "[DB-DIAG] Falha em ping. elapsedMs={ElapsedMs}", sw.ElapsedMilliseconds);
            return Ok(new
            {
                ok = false,
                elapsed_ms = sw.ElapsedMilliseconds,
                error = ex.GetType().Name + ": " + ex.Message,
            });
        }
    }
}

