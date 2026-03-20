using Agile360.API.Models;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingPrazos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Fila de aprovação de prazos enviados pelo bot WhatsApp/n8n.
/// </summary>
[ApiController]
[Route("api/prazo/staging")]
public class StagingPrazoController : ControllerBase
{
    private readonly IStagingPrazoRepository _stagingRepo;
    private readonly IPrazoRepository _prazoRepo;
    private readonly ICurrentUserService _currentUser;

    public StagingPrazoController(
        IStagingPrazoRepository stagingRepo,
        IPrazoRepository prazoRepo,
        ICurrentUserService currentUser)
    {
        _stagingRepo = stagingRepo;
        _prazoRepo = prazoRepo;
        _currentUser = currentUser;
    }

    // ── POST /api/prazo/staging ─────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [EnableCors("ApiIntegration")]
    [ProducesResponseType(typeof(ApiResponse<StagingPrazoResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStagingPrazoRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) && request.DataVencimento is null)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe pelo menos 'titulo' ou 'data_vencimento'.",
                statusCode: 400));
        }

        var item = new StagingPrazo
        {
            Id = Guid.NewGuid(),
            AdvogadoId = _currentUser.AdvogadoId,
            ProcessoId = request.ProcessoId,
            ClienteId = request.ClienteId,
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            TipoPrazo = request.TipoPrazo,
            Prioridade = request.Prioridade,
            DataVencimento = request.DataVencimento,
            DataPublicacao = request.DataPublicacao,
            TipoContagem = request.TipoContagem,
            PrazoDias = request.PrazoDias,
            SuspensaoPrazos = request.SuspensaoPrazos,

            Status = StagingStatus.Pendente,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await _stagingRepo.CreateAsync(item, ct);
        return StatusCode(201, ApiResponse<StagingPrazoResponse>.Ok(Map(item)));
    }

    // ── GET /api/prazo/staging ──────────────────────────────────────────
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingPrazoResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingPrazoResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/prazo/staging/count ────────────────────────────────────
    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingPrazoCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingPrazoCountResponse>.Ok(new StagingPrazoCountResponse(count)));
    }

    // ── PATCH /api/prazo/staging/{id} ───────────────────────────────────
    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingPrazoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AtualizarParcial(
        Guid id,
        [FromBody] UpdateStagingPrazoRequest request,
        CancellationToken ct)
    {
        if (request is null)
            return BadRequest(ApiResponse<object>.Fail("Payload inválido.", statusCode: 400));

        var item = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail("Registro pendente não encontrado.", statusCode: 404));

        if (request.Titulo is not null)
            item.Titulo = request.Titulo;

        if (request.DataVencimento.HasValue)
            item.DataVencimento = request.DataVencimento;

        if (request.Prioridade is not null)
            item.Prioridade = request.Prioridade;

        if (request.TipoContagem is not null)
            item.TipoContagem = request.TipoContagem;

        await _stagingRepo.UpdateAsync(item, ct);
        return Ok(ApiResponse<StagingPrazoResponse>.Ok(Map(item)));
    }

    // ── POST /api/prazo/staging/{id}/confirmar ─────────────────────────
    [HttpPost("{id:guid}/confirmar")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Confirmar(
        Guid id,
        [FromBody] ConfirmarStagingPrazoRequest request,
        CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
        {
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));
        }

        if (string.IsNullOrWhiteSpace(staging.Titulo) || staging.DataVencimento is null)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe 'titulo' e 'data_vencimento' antes de confirmar.",
                statusCode: 400));
        }

        var idCliente = staging.ClienteId ?? request?.IdCliente;
        var idProcesso = staging.ProcessoId ?? request?.IdProcesso;

        if (idCliente is null)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Selecione/identifique um cliente (id_cliente) para vincular o prazo.",
                statusCode: 400));
        }

        var prazo = new Prazo
        {
            Id = Guid.NewGuid(),
            AdvogadoId = _currentUser.AdvogadoId,
            ProcessoId = idProcesso,
            ClienteId = idCliente,
            Titulo = staging.Titulo!,
            Descricao = staging.Descricao,
            TipoPrazo = staging.TipoPrazo ?? "Ordinário",
            Prioridade = MapPrioridade(staging.Prioridade),
            DataVencimento = staging.DataVencimento.Value,
            DataPublicacao = staging.DataPublicacao,
            TipoContagem = MapTipoContagem(staging.TipoContagem),
            Status = "Pendente",
            PrazoDias = staging.PrazoDias,
            SuspensaoPrazos = staging.SuspensaoPrazos,
            LembreteEnviado = false,
        };

        await _prazoRepo.AddAsync(prazo, ct);

        var confirmed = await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, prazo.Id, ct);
        if (!confirmed)
        {
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));
        }

        return Ok(ApiResponse<object>.Ok(new { mensagem = "Prazo confirmado com sucesso.", prazo_id = prazo.Id }));
    }

    // ── DELETE /api/prazo/staging/{id} ─────────────────────────────────
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Rejeitar(Guid id, CancellationToken ct)
    {
        var rejected = await _stagingRepo.RejeitarAsync(id, _currentUser.AdvogadoId, ct);
        if (!rejected)
        {
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));
        }

        return NoContent();
    }

    private static string MapPrioridade(string? prioridade)
    {
        // Triagem usa Normal/Urgente; produção aceita Baixa/Normal/Alta/Fatal.
        if (string.IsNullOrWhiteSpace(prioridade)) return "Normal";

        var p = prioridade.Trim();
        return p.Equals("Urgente", StringComparison.OrdinalIgnoreCase) ? "Alta" : p;
    }

    private static string MapTipoContagem(string? tipoContagem)
    {
        if (string.IsNullOrWhiteSpace(tipoContagem)) return "Util";

        var t = tipoContagem.Trim();
        if (t.Equals("Útil", StringComparison.OrdinalIgnoreCase)) return "Util";
        return t;
    }

    private static StagingPrazoResponse Map(StagingPrazo s) => new(
        Id: s.Id,
        ProcessoId: s.ProcessoId,
        ClienteId: s.ClienteId,
        Titulo: s.Titulo,
        Descricao: s.Descricao,
        TipoPrazo: s.TipoPrazo,
        Prioridade: s.Prioridade,
        DataVencimento: s.DataVencimento,
        DataPublicacao: s.DataPublicacao,
        TipoContagem: s.TipoContagem,
        PrazoDias: s.PrazoDias,
        SuspensaoPrazos: s.SuspensaoPrazos,
        Status: s.Status.ToString(),
        ExpiresAt: s.ExpiresAt,
        CreatedAt: s.CreatedAt
    );
}

