using Agile360.API.Models;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

// ─── DTOs ────────────────────────────────────────────────────────────────────

/// <summary>Payload para POST /api/prazos</summary>
public record CriarPrazoRequest(
    Guid?     IdProcesso,
    Guid      IdCliente,
    string    Titulo,
    string?   Descricao,
    string?   TipoPrazo,
    string    Prioridade,        // Baixa | Normal | Alta | Fatal
    DateOnly? DataPublicacao,
    DateOnly  DataVencimento,
    string    Status,            // Pendente | Concluído | Cancelado
    string    TipoContagem,      // Util | Corrido
    int?      PrazoDias,
    bool      SuspensaoPrazos
);

/// <summary>Payload para PUT /api/prazos/{id}</summary>
public record AtualizarPrazoRequest(
    Guid?     IdProcesso,
    Guid      IdCliente,
    string    Titulo,
    string?   Descricao,
    string?   TipoPrazo,
    string    Prioridade,
    DateOnly? DataPublicacao,
    DateOnly  DataVencimento,
    string    Status,
    string    TipoContagem,
    int?      PrazoDias,
    bool      SuspensaoPrazos,
    DateTimeOffset? DataConclusao   // preenchido ao concluir
);

/// <summary>DTO de resposta para prazos</summary>
public record PrazoResponse(
    Guid     Id,
    Guid     IdAdvogado,
    Guid?    IdProcesso,
    Guid     IdCliente,
    string   Titulo,
    string?  Descricao,
    string?  TipoPrazo,
    string   Prioridade,
    string?  DataPublicacao,   // "yyyy-MM-dd"
    string   DataVencimento,   // "yyyy-MM-dd"
    string?  DataConclusao,    // ISO 8601
    string   Status,
    string   TipoContagem,
    int?     PrazoDias,
    bool     SuspensaoPrazos,
    bool     LembreteEnviado,
    string?  CriadoEm          // ISO 8601
);

// ─── Controller ──────────────────────────────────────────────────────────────

[Authorize]
[ApiController]
[Route("api/prazos")]
public class PrazoController(IPrazoRepository repo) : ControllerBase
{
    private static readonly string[] PrioridadesValidas =
        ["Baixa", "Normal", "Alta", "Fatal"];

    private static readonly string[] StatusValidos =
        ["Pendente", "Concluído", "Cancelado"];

    private static readonly string[] TiposContagemValidos =
        ["Util", "Corrido"];

    // GET /api/prazos
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    // GET /api/prazos/proximos?count=5
    [HttpGet("proximos")]
    public async Task<IActionResult> Proximos([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var lista = await repo.GetProximosAsync(count, ct);
        return Ok(lista.Select(ToResponse));
    }

    // GET /api/prazos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? NotFound() : Ok(ToResponse(p));
    }

    // POST /api/prazos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPrazoRequest req, CancellationToken ct)
    {
        var validacao = Validar(req.Prioridade, req.Status, req.TipoContagem, req.DataVencimento);
        if (validacao is not null) return BadRequest(validacao);

        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    // PUT /api/prazos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPrazoRequest req, CancellationToken ct)
    {
        var validacao = Validar(req.Prioridade, req.Status, req.TipoContagem, req.DataVencimento);
        if (validacao is not null) return BadRequest(validacao);

        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        AplicarAtualizacao(existente, req);
        await repo.UpdateAsync(existente, ct);
        return NoContent();
    }

    // DELETE /api/prazos/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        await repo.RemoveAsync(existente, ct);
        return NoContent();
    }

    // ─── Validações de negócio ────────────────────────────────────────────────

    private ApiResponse<object>? Validar(
        string prioridade, string status, string tipoContagem, DateOnly dataVencimento)
    {
        if (!PrioridadesValidas.Contains(prioridade))
            return ApiResponse<object>.Fail(
                $"Prioridade inválida: '{prioridade}'. Use: {string.Join(", ", PrioridadesValidas)}.");

        if (!StatusValidos.Contains(status))
            return ApiResponse<object>.Fail(
                $"Status inválido: '{status}'. Use: {string.Join(", ", StatusValidos)}.");

        if (!TiposContagemValidos.Contains(tipoContagem))
            return ApiResponse<object>.Fail(
                $"Tipo de contagem inválido: '{tipoContagem}'. Use: Util ou Corrido.");

        if (dataVencimento < DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-10))
            return ApiResponse<object>.Fail("Data de vencimento muito antiga. Verifique o valor informado.");

        return null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Prazo FromCriar(CriarPrazoRequest r) => new()
    {
        IdProcesso      = r.IdProcesso,
        IdCliente       = r.IdCliente,
        Titulo          = r.Titulo,
        Descricao       = r.Descricao,
        TipoPrazo       = r.TipoPrazo,
        Prioridade      = r.Prioridade,
        DataPublicacao  = r.DataPublicacao,
        DataVencimento  = r.DataVencimento,
        Status          = r.Status,
        TipoContagem    = r.TipoContagem,
        PrazoDias       = r.PrazoDias,
        SuspensaoPrazos = r.SuspensaoPrazos,
        LembreteEnviado = false,
        // CriadoEm → DEFAULT now() no Supabase
    };

    private static void AplicarAtualizacao(Prazo p, AtualizarPrazoRequest r)
    {
        p.IdProcesso      = r.IdProcesso;
        p.IdCliente       = r.IdCliente;
        p.Titulo          = r.Titulo;
        p.Descricao       = r.Descricao;
        p.TipoPrazo       = r.TipoPrazo;
        p.Prioridade      = r.Prioridade;
        p.DataPublicacao  = r.DataPublicacao;
        p.DataVencimento  = r.DataVencimento;
        p.DataConclusao   = r.DataConclusao;
        p.Status          = r.Status;
        p.TipoContagem    = r.TipoContagem;
        p.PrazoDias       = r.PrazoDias;
        p.SuspensaoPrazos = r.SuspensaoPrazos;
    }

    private static PrazoResponse ToResponse(Prazo p) => new(
        p.Id,
        p.IdAdvogado,
        p.IdProcesso,
        p.IdCliente,
        p.Titulo,
        p.Descricao,
        p.TipoPrazo,
        p.Prioridade,
        p.DataPublicacao?.ToString("yyyy-MM-dd"),
        p.DataVencimento.ToString("yyyy-MM-dd"),
        p.DataConclusao?.ToString("O"),
        p.Status,
        p.TipoContagem,
        p.PrazoDias,
        p.SuspensaoPrazos,
        p.LembreteEnviado,
        p.CriadoEm?.ToString("O"));
}
