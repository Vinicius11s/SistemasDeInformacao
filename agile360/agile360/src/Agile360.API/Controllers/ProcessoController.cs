using System.Text.RegularExpressions;
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
    // Valores aceitos pelo sistema (espelham o schema Supabase)
    private static readonly string[] StatusValidos =
        ["Ativo", "Suspenso", "Arquivado", "Encerrado"];

    private static readonly string[] FasesValidas =
        ["Conhecimento", "Recursal", "Execução"];

    // GET /api/processos
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    // GET /api/processos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? NotFound() : Ok(ToResponse(p));
    }

    // POST /api/processos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarProcessoRequest req, CancellationToken ct)
    {
        // ── Validações de negócio (SRP: controller valida, repo persiste) ─────

        if (string.IsNullOrWhiteSpace(req.NumProcesso))
            return BadRequest(ApiResponse<object>.Fail(
                "O número do processo é obrigatório."));

        if (!StatusValidos.Contains(req.Status))
            return BadRequest(ApiResponse<object>.Fail(
                $"Status inválido: '{req.Status}'. Use: {string.Join(", ", StatusValidos)}."));

        if (req.FaseProcessual is not null && !FasesValidas.Contains(req.FaseProcessual))
            return BadRequest(ApiResponse<object>.Fail(
                $"Fase processual inválida: '{req.FaseProcessual}'. Use: {string.Join(", ", FasesValidas)}."));

        // Deduplicação antecipada (evita viagem desnecessária ao banco para o caso óbvio)
        var existente = await repo.GetByNumeroAsync(req.NumProcesso.Trim(), ct);
        if (existente is not null)
            return Conflict(ApiResponse<object>.Fail(
                $"Já existe um processo com o número '{req.NumProcesso}'. " +
                "Verifique se você já cadastrou este processo."));

        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    // PUT /api/processos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarProcessoRequest req, CancellationToken ct)
    {
        if (!StatusValidos.Contains(req.Status))
            return BadRequest(ApiResponse<object>.Fail(
                $"Status inválido: '{req.Status}'. Use: {string.Join(", ", StatusValidos)}."));

        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        AplicarAtualizacao(existente, req);
        await repo.UpdateAsync(existente, ct);
        return NoContent();
    }

    // DELETE /api/processos/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        await repo.RemoveAsync(existente, ct);
        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Processo FromCriar(CriarProcessoRequest r) => new()
    {
        IdCliente           = r.IdCliente,
        NumProcesso         = r.NumProcesso.Trim(),
        Status              = r.Status,
        ParteContraria      = r.ParteContraria,
        Tribunal            = r.Tribunal,
        ComarcaVara         = r.ComarcaVara,
        Assunto             = r.Assunto,
        ValorCausa          = r.ValorCausa,
        HonorariosEstimados = r.HonorariosEstimados,
        FaseProcessual      = r.FaseProcessual,
        DataDistribuicao    = r.DataDistribuicao,
        Observacoes         = r.Observacoes,
        // CriadoEm é definido em ProcessoRepository.AddAsync (NOT NULL sem DEFAULT)
    };

    private static void AplicarAtualizacao(Processo p, AtualizarProcessoRequest r)
    {
        p.IdCliente          = r.IdCliente;
        p.NumProcesso        = r.NumProcesso;
        p.Status             = r.Status;
        p.ParteContraria     = r.ParteContraria;
        p.Tribunal           = r.Tribunal;
        p.ComarcaVara        = r.ComarcaVara;
        p.Assunto            = r.Assunto;
        p.ValorCausa         = r.ValorCausa;
        p.HonorariosEstimados= r.HonorariosEstimados;
        p.FaseProcessual     = r.FaseProcessual;
        p.DataDistribuicao   = r.DataDistribuicao;
        p.Observacoes        = r.Observacoes;
    }

    private static ProcessoResponse ToResponse(Processo p) => new(
        p.Id, p.IdAdvogado, p.IdCliente,
        p.NumProcesso, p.Status, p.ParteContraria,
        p.Tribunal, p.ComarcaVara, p.Assunto,
        p.ValorCausa, p.HonorariosEstimados, p.FaseProcessual,
        p.DataDistribuicao, p.Observacoes, p.CriadoEm);
}
