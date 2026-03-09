using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

// ─── Response DTOs ────────────────────────────────────────────────────────────

public record DashboardContadores(
    int AudienciasHoje,
    int AtendimentosHoje,
    int PrazosFatais,       // prazos pendentes com vencimento nos próximos 3 dias
    int NovosProcessosMes   // processos criados no mês atual
);

public record CompromissoDashboard(
    Guid    Id,
    string  Tipo,
    string  Data,       // "yyyy-MM-dd"
    string  Hora,       // "HH:mm"
    string? Local,
    string? IdProcesso
);

public record ProcessoDashboard(
    Guid    Id,
    string  NumProcesso,
    string  Status,
    string? Assunto,
    string? Tribunal,
    string? CriadoEm
);

public record PrazoDashboard(
    Guid    Id,
    string  Titulo,
    string  Status,
    string  Prioridade,
    string  DataVencimento,  // "yyyy-MM-dd"
    string? IdProcesso,
    string? IdCliente
);

public record DashboardResumo(
    DashboardContadores                Contadores,
    IReadOnlyList<CompromissoDashboard> CompromissosSemana,
    IReadOnlyList<ProcessoDashboard>    ProcessosRecentes,
    IReadOnlyList<PrazoDashboard>       PrazosProximos
);

// ─── Controller ──────────────────────────────────────────────────────────────

/// <summary>
/// Agrega dados de Compromissos, Processos e Prazos em uma única chamada.
/// Multi-tenant: cada repositório filtra por id_advogado via Global Query Filter.
/// </summary>
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(
    ICompromissoRepository compromissoRepo,
    IProcessoRepository    processoRepo,
    IPrazoRepository       prazoRepo,
    ILogger<DashboardController> logger) : ControllerBase
{
    // GET /api/dashboard/resumo
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo(CancellationToken ct)
    {
        var hoje      = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMes = new DateOnly(hoje.Year, hoje.Month, 1);

        try
        {
            logger.LogDebug("[Dashboard] Iniciando carregamento de dados para {Data}", hoje);

            // Consultas sequenciais (DbContext não é thread-safe para execução paralela)
            var compromissosHoje   = await compromissoRepo.GetHojeAsync(ct);
            var compromissosSemana = await compromissoRepo.GetSemanaAsync(ct);
            var processosRecentes  = await processoRepo.GetRecentesAsync(5, ct);
            var prazosProximos     = await prazoRepo.GetProximosAsync(5, ct);
            var prazosFatais       = await prazoRepo.GetVencimentoProximoAsync(3, ct);

            logger.LogDebug("[Dashboard] Dados carregados — compromissos hoje: {N1}, semana: {N2}, processos: {N3}, prazos: {N4}",
                compromissosHoje.Count, compromissosSemana.Count, processosRecentes.Count, prazosProximos.Count);

            // tipo_compromisso no banco: "audiencia" | "atendimento" (lowercase)
            var contadores = new DashboardContadores(
                AudienciasHoje:    compromissosHoje.Count(c => string.Equals(c.TipoCompromisso, "audiencia",    StringComparison.OrdinalIgnoreCase)),
                AtendimentosHoje:  compromissosHoje.Count(c => string.Equals(c.TipoCompromisso, "atendimento",  StringComparison.OrdinalIgnoreCase)),
                PrazosFatais:      prazosFatais.Count,
                NovosProcessosMes: processosRecentes.Count(p => p.CriadoEm >= inicioMes)
            );

            var semana = compromissosSemana
                .Select(c => new CompromissoDashboard(
                    c.Id,
                    c.TipoCompromisso,
                    c.Data.ToString("yyyy-MM-dd"),
                    c.Hora.ToString("HH:mm"),
                    c.Local,
                    c.ProcessoId?.ToString()))
                .ToList();

            var recentes = processosRecentes
                .Select(p => new ProcessoDashboard(
                    p.Id,
                    p.NumProcesso,
                    p.Status,
                    p.Assunto,
                    p.Tribunal,
                    p.CriadoEm.ToString("yyyy-MM-dd")))
                .ToList();

            var prazos = prazosProximos
                .Select(p => new PrazoDashboard(
                    p.Id,
                    p.Titulo,
                    p.Status,
                    p.Prioridade,
                    p.DataVencimento.ToString("yyyy-MM-dd"),
                    p.ProcessoId?.ToString(),
                    p.ClienteId?.ToString()))
                .ToList();

            return Ok(new DashboardResumo(contadores, semana, recentes, prazos));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499); // Client Closed Request — browser cancelou antes de responder
        }
        catch (Exception ex) when (IsKnownDatabaseError(ex))
        {
            // Loga o erro REAL para diagnóstico (coluna inexistente, timeout, etc.)
            // e retorna 200 com dados vazios em vez de 500/503 para não travar o frontend.
            logger.LogError(ex, "[Dashboard] Falha na query ao banco — verifique o mapeamento de colunas. Retornando dashboard vazio.");
            return Ok(CreateEmptyResumo());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Dashboard] Erro inesperado ao carregar resumo");
            return StatusCode(500, new { error = new { message = "Erro ao carregar o dashboard. Tente novamente." } });
        }
    }

    private static DashboardResumo CreateEmptyResumo()
    {
        return new DashboardResumo(
            new DashboardContadores(0, 0, 0, 0),
            [],
            [],
            []);
    }

    private static bool IsKnownDatabaseError(Exception ex)
    {
        // Captura: timeout, leitura de stream, erros Npgsql (inclui 42703 coluna inexistente)
        var msg = ex.Message + (ex.InnerException?.Message ?? "");
        return msg.Contains("Timeout",  StringComparison.OrdinalIgnoreCase)
            || msg.Contains("stream",   StringComparison.OrdinalIgnoreCase)
            || msg.Contains("Npgsql",   StringComparison.OrdinalIgnoreCase)
            || msg.Contains("42703",    StringComparison.Ordinal)
            || msg.Contains("column",   StringComparison.OrdinalIgnoreCase);
    }
}
