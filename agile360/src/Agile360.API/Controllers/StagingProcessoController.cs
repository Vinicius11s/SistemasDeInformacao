using Agile360.API.Models;
using Agile360.Application.StagingProcessos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Gerencia a fila de aprovação de processos enviados pelo bot WhatsApp/n8n.
///
/// POST   /api/processos/staging             → n8n (API Key): salva registro pendente
/// GET    /api/processos/staging             → dashboard (JWT): lista pendentes
/// GET    /api/processos/staging/count       → dashboard (JWT): badge de notificação
/// POST   /api/processos/staging/{id}/confirmar → dashboard (JWT): promove para processos
/// DELETE /api/processos/staging/{id}        → dashboard (JWT): rejeita/descarta
/// </summary>
[ApiController]
[Route("api/processos/staging")]
[Authorize]
public class StagingProcessoController : ControllerBase
{
    private readonly IStagingProcessoRepository _stagingRepo;
    private readonly ICurrentUserService        _currentUser;

    public StagingProcessoController(
        IStagingProcessoRepository stagingRepo,
        ICurrentUserService currentUser)
    {
        _stagingRepo = stagingRepo;
        _currentUser = currentUser;
    }

    // ── POST /api/processos/staging ───────────────────────────────────────
    // Chamado pelo bot n8n com API Key. Cria um registro Pendente.

    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [ProducesResponseType(typeof(ApiResponse<StagingProcessoResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStagingProcessoRequest request,
        CancellationToken ct)
    {
        // Guard mínimo: ao menos número do processo ou parte contrária devem estar presentes
        if (string.IsNullOrWhiteSpace(request.NumProcesso) &&
            string.IsNullOrWhiteSpace(request.ParteContraria))
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe pelo menos o Número do Processo ou a Parte Contrária.", statusCode: 400));
        }

        var item = new StagingProcesso
        {
            Id                  = Guid.NewGuid(),
            AdvogadoId          = _currentUser.AdvogadoId,
            NumProcesso         = request.NumProcesso,
            ParteContraria      = request.ParteContraria,
            Tribunal            = request.Tribunal,
            ComarcaVara         = request.ComarcaVara,
            Assunto             = request.Assunto,
            ValorCausa          = request.ValorCausa,
            HonorariosEstimados = request.HonorariosEstimados,
            FaseProcessual      = request.FaseProcessual,
            StatusProcesso      = request.StatusProcesso,
            DataDistribuicao    = request.DataDistribuicao,
            ClienteNome         = request.ClienteNome,
            Observacoes         = request.Observacoes,
            OrigemMensagem      = request.OrigemMensagem,
            Status              = StagingStatus.Pendente,
            ExpiresAt           = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt           = DateTimeOffset.UtcNow,
            UpdatedAt           = DateTimeOffset.UtcNow,
        };

        await _stagingRepo.CreateAsync(item, ct);
        return StatusCode(201, ApiResponse<StagingProcessoResponse>.Ok(Map(item)));
    }

    // ── GET /api/processos/staging ────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingProcessoResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingProcessoResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/processos/staging/count ──────────────────────────────────

    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<StagingProcessoCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingProcessoCountResponse>.Ok(new StagingProcessoCountResponse(count)));
    }

    // ── POST /api/processos/staging/{id}/confirmar ────────────────────────
    // Dashboard: promove staging → processo (a ser implementado pelo @dev quando
    // o fluxo de criação de Processo estiver definido).

    [HttpPost("{id:guid}/confirmar")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        // TODO (próxima story): criar o Processo em produção aqui,
        // vincular ao Cliente existente e chamar ConfirmarAsync com o Guid gerado.
        // Por ora, confirma o staging sem criar o processo de produção.
        var confirmed = await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, Guid.Empty, ct);
        if (!confirmed)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        return Ok(ApiResponse<object>.Ok(new { mensagem = "Processo confirmado e aguardando vinculação." }));
    }

    // ── DELETE /api/processos/staging/{id} ───────────────────────────────

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

    private static StagingProcessoResponse Map(StagingProcesso s) => new(
        s.Id,
        s.NumProcesso,
        s.ParteContraria,
        s.Tribunal,
        s.ComarcaVara,
        s.Assunto,
        s.ValorCausa,
        s.HonorariosEstimados,
        s.FaseProcessual,
        s.StatusProcesso,
        s.DataDistribuicao,
        s.ClienteNome,
        s.Observacoes,
        s.OrigemMensagem,
        s.Status.ToString(),
        s.ExpiresAt,
        s.CreatedAt);
}
