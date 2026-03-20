using Agile360.API.Models;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingProcessos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Gerencia a fila de aprovação de processos enviados pelo bot WhatsApp/n8n.
///
/// POST   /api/processo/staging             → n8n (API Key): salva registro pendente
/// GET    /api/processo/staging             → dashboard (JWT): lista pendentes
/// GET    /api/processo/staging/count       → dashboard (JWT): badge de notificação
/// POST   /api/processo/staging/{id}/confirmar → dashboard (JWT): promove para processos
/// DELETE /api/processo/staging/{id}        → dashboard (JWT): rejeita/descarta
/// </summary>
[ApiController]
[Route("api/processo/staging")]
public class StagingProcessoController : ControllerBase
{
    private readonly IStagingProcessoRepository _stagingRepo;
    private readonly IProcessoRepository      _processoRepo;
    private readonly IClienteRepository       _clienteRepo;
    private readonly ICurrentUserService     _currentUser;

    public StagingProcessoController(
        IStagingProcessoRepository stagingRepo,
        ICurrentUserService currentUser,
        IProcessoRepository processoRepo,
        IClienteRepository clienteRepo)
    {
        _stagingRepo = stagingRepo;
        _processoRepo = processoRepo;
        _clienteRepo = clienteRepo;
        _currentUser = currentUser;
    }

    public record ConfirmarStagingProcessoRequest(Guid? IdCliente);

    // ── POST /api/processo/staging ───────────────────────────────────────
    // Chamado pelo bot n8n com API Key. Cria um registro Pendente.

    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [EnableCors("ApiIntegration")]
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
            NumProcesso         = NormalizeNumProcesso(request.NumProcesso),
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

    // ── GET /api/processo/staging ────────────────────────────────────────

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingProcessoResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingProcessoResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/processo/staging/count ──────────────────────────────────

    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingProcessoCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingProcessoCountResponse>.Ok(new StagingProcessoCountResponse(count)));
    }

    // ── PATCH /api/processo/staging/{id} ──────────────────────────────
    // Dashboard: permite ao advogado editar campos antes de confirmar.
    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingProcessoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AtualizarParcial(Guid id, [FromBody] UpdateStagingProcessoRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(ApiResponse<object>.Fail("Payload inválido.", statusCode: 400));

        var item = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail("Registro pendente não encontrado.", statusCode: 404));

        if (request.NumProcesso is not null)
            item.NumProcesso = NormalizeNumProcesso(request.NumProcesso);

        if (request.ParteContraria is not null)
            item.ParteContraria = request.ParteContraria;

        if (request.ValorCausa.HasValue)
            item.ValorCausa = request.ValorCausa;

        if (request.Tribunal is not null)
            item.Tribunal = request.Tribunal;

        if (request.ComarcaVara is not null)
            item.ComarcaVara = request.ComarcaVara;

        if (request.Assunto is not null)
            item.Assunto = request.Assunto;

        await _stagingRepo.UpdateAsync(item, ct);

        return Ok(ApiResponse<StagingProcessoResponse>.Ok(Map(item)));
    }

    // ── POST /api/processo/staging/{id}/confirmar ────────────────────────
    // Dashboard: promove staging → processo (cria o registro em produção).

    [HttpPost("{id:guid}/confirmar")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Confirmar(Guid id, [FromBody] ConfirmarStagingProcessoRequest? request, CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        if (string.IsNullOrWhiteSpace(staging.NumProcesso))
            return BadRequest(ApiResponse<object>.Fail("Número do processo inválido.", statusCode: 400));

        Guid? idCliente = request?.IdCliente;

        // Tentativa de resolução automática via CPF (quando o bot manda um identificador em ClienteNome).
        if (idCliente is null && !string.IsNullOrWhiteSpace(staging.ClienteNome))
        {
            var cpf = Agile360.Shared.DocumentSanitizer.Sanitize(staging.ClienteNome);
            if (!string.IsNullOrWhiteSpace(cpf))
            {
                if (cpf!.Length == 11)
                {
                    var cliente = await _clienteRepo.GetByCpfAsync(cpf, ct);
                    if (cliente is not null) idCliente = cliente.Id;
                }
            }
        }

        if (idCliente is null)
            return BadRequest(ApiResponse<object>.Fail(
                "Selecione/identifique um cliente (id_cliente) para vincular este processo.", statusCode: 400));

        var numProcessoCanonical = NormalizeNumProcesso(staging.NumProcesso);

        var existente = await _processoRepo.GetByNumeroAsync(numProcessoCanonical, ct);
        if (existente is not null)
            return Conflict(ApiResponse<object>.Fail(
                $"Já existe um processo com o número '{numProcessoCanonical}'.", statusCode: 409));

        var processo = new Processo
        {
            Id = Guid.NewGuid(),
            AdvogadoId = _currentUser.AdvogadoId,
            ClienteId = idCliente,
            NumProcesso = numProcessoCanonical,
            ParteContraria = staging.ParteContraria,
            Tribunal = staging.Tribunal,
            ComarcaVara = staging.ComarcaVara,
            Assunto = staging.Assunto,
            ValorCausa = staging.ValorCausa,
            HonorariosEstimados = staging.HonorariosEstimados,
            FaseProcessual = staging.FaseProcessual,
            Status = staging.StatusProcesso ?? "Ativo",
            DataDistribuicao = staging.DataDistribuicao,
            Observacoes = staging.Observacoes,
        };

        await _processoRepo.AddAsync(processo, ct);

        var confirmed = await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, processo.Id, ct);
        if (!confirmed)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        return Ok(ApiResponse<object>.Ok(new { mensagem = "Processo confirmado com sucesso.", processo_id = processo.Id }));
    }

    // ── Normalização CNJ (remove pontuação e reformat para padrão canônico) ──
    // CNJ (antigo) esperado: NNNNNNN-DD.AAAA.J.T.OOOO → 20 dígitos.
    // Ex.: 0000000-00.2026.8.26.0000
    private static string NormalizeNumProcesso(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var digits = Agile360.Shared.DocumentSanitizer.OnlyDigits(input);
        if (string.IsNullOrWhiteSpace(digits)) return input.Trim();

        if (digits.Length != 20) return digits;

        var n7 = digits[..7];
        var dd = digits.Substring(7, 2);
        var yyyy = digits.Substring(9, 4);
        var j = digits[13];
        var t = digits.Substring(14, 2);
        var oooo = digits.Substring(16, 4);

        return $"{n7}-{dd}.{yyyy}.{j}.{t}.{oooo}";
    }

    // ── DELETE /api/processo/staging/{id} ───────────────────────────────

    [HttpDelete("{id:guid}")]
    [Authorize]
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
