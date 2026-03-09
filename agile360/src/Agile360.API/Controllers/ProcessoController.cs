using Agile360.API.Models;
using Agile360.Application.Processos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

[Authorize]
[ApiController]
[Route("api/processos")]
public class ProcessoController(IProcessoRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? NotFound() : Ok(ToResponse(p));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateProcessoRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NumProcesso))
            return BadRequest(ApiResponse<object>.Fail("Número do processo é obrigatório."));

        var existente = await repo.GetByNumeroAsync(req.NumProcesso.Trim(), ct);
        if (existente is not null)
            return Conflict(ApiResponse<object>.Fail(
                $"Já existe um processo com o número '{req.NumProcesso}'."));

        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateProcessoRequest req, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();
        AplicarAtualizacao(existente, req);
        await repo.UpdateAsync(existente, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();
        await repo.RemoveAsync(existente, ct);
        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Processo FromCriar(CreateProcessoRequest r) => new()
    {
        ClienteId           = r.IdCliente,
        NumProcesso         = r.NumProcesso.Trim(),
        ParteContraria      = r.ParteContraria,
        Tribunal            = r.Tribunal,
        ComarcaVara         = r.ComarcaVara,
        Assunto             = r.Assunto,
        ValorCausa          = r.ValorCausa,
        HonorariosEstimados = r.HonorariosEstimados,
        FaseProcessual      = r.FaseProcessual,
        Status              = r.Status,
        DataDistribuicao    = r.DataDistribuicao,
        Observacoes         = r.Observacoes,
    };

    private static void AplicarAtualizacao(Processo p, UpdateProcessoRequest r)
    {
        if (r.IdCliente.HasValue)           p.ClienteId           = r.IdCliente;
        if (r.NumProcesso        != null)   p.NumProcesso         = r.NumProcesso;
        if (r.ParteContraria     != null)   p.ParteContraria      = r.ParteContraria;
        if (r.Tribunal           != null)   p.Tribunal            = r.Tribunal;
        if (r.ComarcaVara        != null)   p.ComarcaVara         = r.ComarcaVara;
        if (r.Assunto            != null)   p.Assunto             = r.Assunto;
        if (r.ValorCausa.HasValue)          p.ValorCausa          = r.ValorCausa;
        if (r.HonorariosEstimados.HasValue) p.HonorariosEstimados = r.HonorariosEstimados;
        if (r.FaseProcessual     != null)   p.FaseProcessual      = r.FaseProcessual;
        if (r.Status             != null)   p.Status              = r.Status;
        if (r.DataDistribuicao.HasValue)    p.DataDistribuicao    = r.DataDistribuicao;
        if (r.Observacoes        != null)   p.Observacoes         = r.Observacoes;
    }

    private static ProcessoResponse ToResponse(Processo p) => new(
        Id:                  p.Id,
        IdCliente:           p.ClienteId,
        NumProcesso:         p.NumProcesso,
        ParteContraria:      p.ParteContraria,
        Tribunal:            p.Tribunal,
        ComarcaVara:         p.ComarcaVara,
        Assunto:             p.Assunto,
        ValorCausa:          p.ValorCausa,
        HonorariosEstimados: p.HonorariosEstimados,
        FaseProcessual:      p.FaseProcessual,
        Status:              p.Status,
        DataDistribuicao:    p.DataDistribuicao,
        CriadoEm:            p.CriadoEm,
        Observacoes:         p.Observacoes);
}
