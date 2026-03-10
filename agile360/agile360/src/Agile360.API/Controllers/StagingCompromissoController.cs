using Agile360.API.Models;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingCompromissos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Gerencia a fila de aprovação de compromissos enviados pelo bot WhatsApp/n8n.
///
/// POST   /api/compromissos/staging             → n8n (API Key): salva registro pendente
/// GET    /api/compromissos/staging             → dashboard (JWT): lista pendentes
/// GET    /api/compromissos/staging/count       → dashboard (JWT): badge de notificação
/// POST   /api/compromissos/staging/{id}/confirmar → dashboard (JWT): promove para compromissos
/// DELETE /api/compromissos/staging/{id}        → dashboard (JWT): rejeita/descarta
/// </summary>
[ApiController]
[Route("api/compromissos/staging")]
[Authorize]
public class StagingCompromissoController : ControllerBase
{
    private readonly IStagingCompromissoRepository _stagingRepo;
    private readonly ICurrentUserService           _currentUser;

    public StagingCompromissoController(
        IStagingCompromissoRepository stagingRepo,
        ICurrentUserService currentUser)
    {
        _stagingRepo = stagingRepo;
        _currentUser = currentUser;
    }

    // ── POST /api/compromissos/staging ────────────────────────────────────
    // Chamado pelo bot n8n com API Key. Cria um registro Pendente.

    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [ProducesResponseType(typeof(ApiResponse<StagingCompromissoResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStagingCompromissoRequest request,
        CancellationToken ct)
    {
        // Guard mínimo: ao menos tipo ou data devem estar presentes
        if (string.IsNullOrWhiteSpace(request.TipoCompromisso) && request.Data is null)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe pelo menos o Tipo do Compromisso ou a Data.", statusCode: 400));
        }

        var item = new StagingCompromisso
        {
            Id              = Guid.NewGuid(),
            AdvogadoId      = _currentUser.AdvogadoId,
            TipoCompromisso = request.TipoCompromisso,
            TipoAudiencia   = request.TipoAudiencia,
            Data            = request.Data,
            Hora            = request.Hora,
            Local           = request.Local,
            ClienteNome     = request.ClienteNome,
            NumProcesso     = request.NumProcesso,
            Observacoes     = request.Observacoes,
            LembreteMinutos = request.LembreteMinutos,
            OrigemMensagem  = request.OrigemMensagem,
            Status          = StagingStatus.Pendente,
            ExpiresAt       = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow,
        };

        await _stagingRepo.CreateAsync(item, ct);
        return StatusCode(201, ApiResponse<StagingCompromissoResponse>.Ok(Map(item)));
    }

    // ── GET /api/compromissos/staging ─────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingCompromissoResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingCompromissoResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/compromissos/staging/count ───────────────────────────────

    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<StagingCompromissoCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingCompromissoCountResponse>.Ok(new StagingCompromissoCountResponse(count)));
    }

    // ── POST /api/compromissos/staging/{id}/confirmar ─────────────────────
    // Dashboard: promove staging → compromisso (a ser implementado pelo @dev quando
    // o fluxo de criação de Compromisso estiver definido).

    [HttpPost("{id:guid}/confirmar")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        // TODO (próxima story): criar o Compromisso em produção aqui,
        // vincular ao Cliente/Processo existente e chamar ConfirmarAsync com o Guid gerado.
        // Por ora, confirma o staging sem criar o compromisso de produção.
        var confirmed = await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, Guid.Empty, ct);
        if (!confirmed)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        return Ok(ApiResponse<object>.Ok(new { mensagem = "Compromisso confirmado e aguardando vinculação." }));
    }

    // ── DELETE /api/compromissos/staging/{id} ─────────────────────────────

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Rejeitar(Guid id, CancellationToken ct)
    {
        var rejected = await _stagingRepo.RejeitarAsync(id, _currentUser.AdvogadoId, ct);
        if (!rejected)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        return NoContent();
    }

    // ── Mapper privado ────────────────────────────────────────────────────

    private static StagingCompromissoResponse Map(StagingCompromisso s) => new(
        s.Id,
        s.TipoCompromisso,
        s.TipoAudiencia,
        s.Data,
        s.Hora,
        s.Local,
        s.ClienteNome,
        s.NumProcesso,
        s.Observacoes,
        s.LembreteMinutos,
        s.OrigemMensagem,
        s.Status.ToString(),
        s.ExpiresAt,
        s.CreatedAt);
}
