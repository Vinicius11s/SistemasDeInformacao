using Agile360.API.Models;
using Agile360.Application.Compromissos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

[Authorize]
[ApiController]
[Route("api/compromissos")]
public class CompromissoController(ICompromissoRepository repo) : ControllerBase
{
    private static readonly string[] TiposValidos =
        ["Audiência", "Atendimento", "Reunião", "Prazo"];

    private static readonly string[] StatusValidos =
        ["Agendado", "Concluído", "Cancelado"];

    // GET /api/compromissos
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    // GET /api/compromissos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(id, ct);
        return c is null ? NotFound() : Ok(ToResponse(c));
    }

    // POST /api/compromissos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCompromissoRequest req, CancellationToken ct)
    {
        // ── Validações de negócio (SRP: controller valida, repo persiste) ─────

        if (!TiposValidos.Contains(req.TipoCompromisso))
            return BadRequest(ApiResponse<object>.Fail(
                $"Tipo de compromisso inválido: '{req.TipoCompromisso}'. " +
                $"Use: {string.Join(", ", TiposValidos)}."));

        if (!StatusValidos.Contains(req.Status))
            return BadRequest(ApiResponse<object>.Fail(
                $"Status inválido: '{req.Status}'. Use: {string.Join(", ", StatusValidos)}."));

        // Audiência obrigatoriamente vinculada a um processo
        if (req.TipoCompromisso == "Audiência" && req.IdProcesso is null)
            return BadRequest(ApiResponse<object>.Fail(
                "Compromissos do tipo 'Audiência' devem estar vinculados a um processo (id_processo obrigatório)."));

        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    // PUT /api/compromissos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCompromissoRequest req, CancellationToken ct)
    {
        if (!TiposValidos.Contains(req.TipoCompromisso))
            return BadRequest(ApiResponse<object>.Fail(
                $"Tipo de compromisso inválido: '{req.TipoCompromisso}'."));

        if (!StatusValidos.Contains(req.Status))
            return BadRequest(ApiResponse<object>.Fail(
                $"Status inválido: '{req.Status}'."));

        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        AplicarAtualizacao(existente, req);
        await repo.UpdateAsync(existente, ct);
        return NoContent();
    }

    // DELETE /api/compromissos/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        await repo.RemoveAsync(existente, ct);
        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Compromisso FromCriar(CriarCompromissoRequest r) => new()
    {
        TipoCompromisso = r.TipoCompromisso,
        TipoAudiencia   = r.TipoAudiencia,
        Status          = r.Status,
        Data            = r.Data,
        Hora            = r.Hora,
        Local           = r.Local,
        IdCliente       = r.IdCliente,
        IdProcesso      = r.IdProcesso,
        Observacoes     = r.Observacoes,
        LembreteMinutos = r.LembreteMinutos,
        // CriadoEm é definido em CompromissoRepository.AddAsync (NOT NULL sem DEFAULT)
    };

    private static void AplicarAtualizacao(Compromisso c, AtualizarCompromissoRequest r)
    {
        c.TipoCompromisso = r.TipoCompromisso;
        c.TipoAudiencia   = r.TipoAudiencia;
        c.Status          = r.Status;
        c.Data            = r.Data;
        c.Hora            = r.Hora;
        c.Local           = r.Local;
        c.IdCliente       = r.IdCliente;
        c.IdProcesso      = r.IdProcesso;
        c.Observacoes     = r.Observacoes;
        c.LembreteMinutos = r.LembreteMinutos;
    }

    private static CompromissoResponse ToResponse(Compromisso c) => new(
        c.Id, c.IdAdvogado,
        c.TipoCompromisso, c.TipoAudiencia, c.Status,
        c.Data, c.Hora, c.Local,
        c.IdCliente, c.IdProcesso,
        c.Observacoes, c.LembreteMinutos, c.CriadoEm);
}
