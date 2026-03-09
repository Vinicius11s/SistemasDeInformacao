using Agile360.API.Models;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public record CriarPrazoRequest(
    Guid?    ProcessoId,
    Guid?    ClienteId,
    string   Titulo,
    string?  Descricao,
    string   TipoPrazo,
    string   Prioridade,
    DateOnly DataVencimento,
    DateOnly? DataPublicacao
);

public record AtualizarPrazoRequest(
    string?         Titulo,
    string?         Descricao,
    string?         TipoPrazo,
    string?         Prioridade,
    string?         Status,
    DateOnly?       DataVencimento,
    DateTimeOffset? DataConclusao
);

public record PrazoResponse(
    Guid            Id,
    Guid            AdvogadoId,
    Guid?           ProcessoId,
    Guid?           ClienteId,
    string          Titulo,
    string?         Descricao,
    string          TipoPrazo,
    string          Prioridade,
    string          Status,
    DateOnly        DataVencimento,
    DateOnly?       DataPublicacao,
    DateTimeOffset? DataConclusao,
    bool            LembreteEnviado,
    DateTimeOffset? CriadoEm
);

// ─── Controller ──────────────────────────────────────────────────────────────

[Authorize]
[ApiController]
[Route("api/prazos")]
public class PrazoController(IPrazoRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    [HttpGet("proximos")]
    public async Task<IActionResult> Proximos([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var lista = await repo.GetProximosAsync(count, ct);
        return Ok(lista.Select(ToResponse));
    }

    [HttpGet("pendentes")]
    public async Task<IActionResult> Pendentes(CancellationToken ct)
    {
        var lista = await repo.GetPendentesAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? NotFound() : Ok(ToResponse(p));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPrazoRequest req, CancellationToken ct)
    {
        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPrazoRequest req, CancellationToken ct)
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

    private static Prazo FromCriar(CriarPrazoRequest r) => new()
    {
        ProcessoId     = r.ProcessoId,
        ClienteId      = r.ClienteId,
        Titulo         = r.Titulo,
        Descricao      = r.Descricao,
        TipoPrazo      = r.TipoPrazo,
        Prioridade     = r.Prioridade,
        DataVencimento = r.DataVencimento,
        DataPublicacao = r.DataPublicacao,
        Status         = "Pendente",
        LembreteEnviado = false,
    };

    private static void AplicarAtualizacao(Prazo p, AtualizarPrazoRequest r)
    {
        if (r.Titulo         != null) p.Titulo         = r.Titulo;
        if (r.Descricao      != null) p.Descricao      = r.Descricao;
        if (r.TipoPrazo      != null) p.TipoPrazo      = r.TipoPrazo;
        if (r.Prioridade     != null) p.Prioridade     = r.Prioridade;
        if (r.Status         != null) p.Status         = r.Status;
        if (r.DataVencimento.HasValue) p.DataVencimento = r.DataVencimento.Value;
        if (r.DataConclusao.HasValue)  p.DataConclusao  = r.DataConclusao;
    }

    private static PrazoResponse ToResponse(Prazo p) => new(
        Id:              p.Id,
        AdvogadoId:      p.AdvogadoId,
        ProcessoId:      p.ProcessoId,
        ClienteId:       p.ClienteId,
        Titulo:          p.Titulo,
        Descricao:       p.Descricao,
        TipoPrazo:       p.TipoPrazo,
        Prioridade:      p.Prioridade,
        Status:          p.Status,
        DataVencimento:  p.DataVencimento,
        DataPublicacao:  p.DataPublicacao,
        DataConclusao:   p.DataConclusao,
        LembreteEnviado: p.LembreteEnviado,
        CriadoEm:        p.CriadoEm);
}
