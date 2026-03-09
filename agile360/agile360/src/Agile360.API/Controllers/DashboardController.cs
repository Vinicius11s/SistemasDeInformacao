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
    string  Status,
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
/// Todas as queries são executadas em paralelo com Task.WhenAll.
/// Multi-tenant: cada repositório filtra por id_advogado via RLS.
/// </summary>
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(
    ICompromissoRepository compromissoRepo,
    IProcessoRepository    processoRepo,
    IPrazoRepository       prazoRepo) : ControllerBase
{
    // GET /api/dashboard/resumo
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo(CancellationToken ct)
    {
        var hoje      = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMes = new DateOnly(hoje.Year, hoje.Month, 1);

        // ── Consultas em paralelo ──────────────────────────────────────────
        var taskHoje         = compromissoRepo.GetHojeAsync(ct);
        var taskSemana       = compromissoRepo.GetSemanaAsync(ct);
        var taskRecentes     = processoRepo.GetRecentesAsync(5, ct);
        var taskPrazosProx   = prazoRepo.GetProximosAsync(5, ct);
        var taskPrazosFatais = prazoRepo.GetVencimentoProximoAsync(3, ct);

        await Task.WhenAll(taskHoje, taskSemana, taskRecentes, taskPrazosProx, taskPrazosFatais);

        var compromissosHoje   = taskHoje.Result;
        var compromissosSemana = taskSemana.Result;
        var processosRecentes  = taskRecentes.Result;
        var prazosProximos     = taskPrazosProx.Result;
        var prazosFatais       = taskPrazosFatais.Result;

        // ── Contadores ────────────────────────────────────────────────────
        var contadores = new DashboardContadores(
            AudienciasHoje:    compromissosHoje.Count(c => c.TipoCompromisso == "Audiência"),
            AtendimentosHoje:  compromissosHoje.Count(c => c.TipoCompromisso == "Atendimento"),
            PrazosFatais:      prazosFatais.Count,   // ← vem da tabela prazo
            NovosProcessosMes: processosRecentes.Count(p =>
                                   p.CriadoEm.HasValue && p.CriadoEm.Value >= inicioMes)
        );

        // ── Mapeamento ────────────────────────────────────────────────────
        var semana = compromissosSemana
            .Select(c => new CompromissoDashboard(
                c.Id,
                c.TipoCompromisso,
                c.Status,
                c.Data.ToString("yyyy-MM-dd"),
                c.Hora.ToString("HH:mm"),
                c.Local,
                c.IdProcesso?.ToString()))
            .ToList();

        var recentes = processosRecentes
            .Select(p => new ProcessoDashboard(
                p.Id,
                p.NumProcesso,
                p.Status,
                p.Assunto,
                p.Tribunal,
                p.CriadoEm?.ToString("yyyy-MM-dd")))
            .ToList();

        var prazos = prazosProximos
            .Select(p => new PrazoDashboard(
                p.Id,
                p.Titulo,
                p.Status,
                p.Prioridade,
                p.DataVencimento.ToString("yyyy-MM-dd"),
                p.IdProcesso?.ToString(),
                p.IdCliente.ToString()))
            .ToList();

        return Ok(new DashboardResumo(contadores, semana, recentes, prazos));
    }
}
