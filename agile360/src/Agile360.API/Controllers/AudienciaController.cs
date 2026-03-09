using Agile360.API.Models;
using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

// ─── DTOs ─────────────────────────────────────────────────────────────────────
// AudienciaDto.cs foi removido — Audiencia não é mais entidade separada.
// Compromisso com tipo_compromisso = "audiencia" representa audiências.

public record AudienciaResponse(
    Guid     Id,
    Guid?    ProcessoId,
    string   Data,          // "yyyy-MM-dd"
    string   Hora,          // "HH:mm"
    string?  TipoAudiencia, // subtipo: "Conciliação", "Instrução", etc.
    string?  Local,
    string?  Observacoes,
    bool     IsActive
);

public record CreateAudienciaRequest(
    Guid?    ProcessoId,
    Guid?    ClienteId,
    string   Data,          // "yyyy-MM-dd"
    string   Hora,          // "HH:mm"
    string?  TipoAudiencia,
    string?  Local,
    string?  Observacoes,
    int?     LembreteMinutos
);

public record UpdateAudienciaRequest(
    string?  Data,
    string?  Hora,
    string?  TipoAudiencia,
    string?  Local,
    string?  Observacoes,
    int?     LembreteMinutos
);

// ─── Controller ───────────────────────────────────────────────────────────────

/// <summary>
/// Audiências são compromissos com tipo_compromisso = "audiencia".
/// Toda persistência vai para a tabela "compromisso".
/// </summary>
[ApiController]
[Route("api/audiencias")]
[Authorize]
public class AudienciaController(ICompromissoRepository repo, ICurrentUserService currentUser) : ControllerBase
{
    private const string TipoAudiencia = "audiencia";

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<AudienciaResponse>>.Ok(
            items.Where(c => c.TipoCompromisso == TipoAudiencia).Select(Map).ToList()));
    }

    [HttpGet("hoje")]
    public async Task<IActionResult> Hoje(CancellationToken ct)
    {
        var items = await repo.GetHojeAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<AudienciaResponse>>.Ok(
            items.Where(c => c.TipoCompromisso == TipoAudiencia).Select(Map).ToList()));
    }

    [HttpGet("semana")]
    public async Task<IActionResult> Semana(CancellationToken ct)
    {
        var items = await repo.GetSemanaAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<AudienciaResponse>>.Ok(
            items.Where(c => c.TipoCompromisso == TipoAudiencia).Select(Map).ToList()));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(id, ct);
        if (c == null || c.TipoCompromisso != TipoAudiencia)
            return NotFound(ApiResponse<object>.Fail("Audiência não encontrada.", statusCode: 404));
        return Ok(ApiResponse<AudienciaResponse>.Ok(Map(c)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAudienciaRequest req, CancellationToken ct)
    {
        if (!DateOnly.TryParse(req.Data, out var data))
            return BadRequest(ApiResponse<object>.Fail("Data inválida. Use formato yyyy-MM-dd."));
        if (!TimeOnly.TryParse(req.Hora, out var hora))
            return BadRequest(ApiResponse<object>.Fail("Hora inválida. Use formato HH:mm."));

        var compromisso = new Compromisso
        {
            Id              = Guid.NewGuid(),
            AdvogadoId      = currentUser.AdvogadoId,
            ProcessoId      = req.ProcessoId,
            ClienteId       = req.ClienteId,
            TipoCompromisso = TipoAudiencia,
            TipoAudiencia   = req.TipoAudiencia,
            Data            = data,
            Hora            = hora,
            Local           = req.Local,
            Observacoes     = req.Observacoes,
            LembreteMinutos = req.LembreteMinutos,
            IsActive        = true,
        };

        await repo.AddAsync(compromisso, ct);
        return StatusCode(201, ApiResponse<AudienciaResponse>.Ok(Map(compromisso)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAudienciaRequest req, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(id, ct);
        if (c == null || c.TipoCompromisso != TipoAudiencia)
            return NotFound(ApiResponse<object>.Fail("Audiência não encontrada.", statusCode: 404));

        if (req.Data != null && DateOnly.TryParse(req.Data, out var data))   c.Data           = data;
        if (req.Hora != null && TimeOnly.TryParse(req.Hora, out var hora))   c.Hora           = hora;
        if (req.TipoAudiencia   != null) c.TipoAudiencia   = req.TipoAudiencia;
        if (req.Local           != null) c.Local           = req.Local;
        if (req.Observacoes     != null) c.Observacoes     = req.Observacoes;
        if (req.LembreteMinutos != null) c.LembreteMinutos = req.LembreteMinutos;

        await repo.UpdateAsync(c, ct);
        return Ok(ApiResponse<AudienciaResponse>.Ok(Map(c)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(id, ct);
        if (c == null || c.TipoCompromisso != TipoAudiencia)
            return NotFound(ApiResponse<object>.Fail("Audiência não encontrada.", statusCode: 404));

        await repo.RemoveAsync(c, ct);
        return NoContent();
    }

    private static AudienciaResponse Map(Compromisso c) => new(
        c.Id, c.ProcessoId,
        c.Data.ToString("yyyy-MM-dd"),
        c.Hora.ToString("HH:mm"),
        c.TipoAudiencia,
        c.Local,
        c.Observacoes,
        c.IsActive);
}
