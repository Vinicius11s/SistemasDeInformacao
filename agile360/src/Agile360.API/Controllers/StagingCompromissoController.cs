using Agile360.API.Models;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingCompromissos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Gerencia a fila de aprovação de compromissos enviados pelo bot WhatsApp/n8n.
///
/// POST   /api/compromisso/staging             → n8n (API Key): salva registro pendente
/// GET    /api/compromisso/staging             → dashboard (JWT): lista pendentes
/// GET    /api/compromisso/staging/count       → dashboard (JWT): badge de notificação
/// POST   /api/compromisso/staging/{id}/confirmar → dashboard (JWT): promove para compromissos
/// DELETE /api/compromisso/staging/{id}        → dashboard (JWT): rejeita/descarta
/// </summary>
[ApiController]
[Route("api/compromisso/staging")]
public class StagingCompromissoController : ControllerBase
{
    private readonly IStagingCompromissoRepository _stagingRepo;
    private readonly ICompromissoRepository         _compromissoRepo;
    private readonly IProcessoRepository            _processoRepo;
    private readonly IClienteRepository            _clienteRepo;
    private readonly ICurrentUserService           _currentUser;

    public StagingCompromissoController(
        IStagingCompromissoRepository stagingRepo,
        ICurrentUserService currentUser,
        ICompromissoRepository compromissoRepo,
        IProcessoRepository processoRepo,
        IClienteRepository clienteRepo)
    {
        _stagingRepo = stagingRepo;
        _compromissoRepo = compromissoRepo;
        _processoRepo = processoRepo;
        _clienteRepo = clienteRepo;
        _currentUser = currentUser;
    }

    public record ConfirmarStagingCompromissoRequest(Guid? IdCliente, Guid? IdProcesso);

    // ── POST /api/compromisso/staging ────────────────────────────────────
    // Chamado pelo bot n8n com API Key. Cria um registro Pendente.

    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [EnableCors("ApiIntegration")]
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

    // ── GET /api/compromisso/staging ─────────────────────────────────────

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingCompromissoResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingCompromissoResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/compromisso/staging/count ───────────────────────────────

    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingCompromissoCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingCompromissoCountResponse>.Ok(new StagingCompromissoCountResponse(count)));
    }

    // ── PATCH /api/compromisso/staging/{id} ─────────────────────────────
    // Dashboard: permite ao advogado editar campos antes de confirmar.
    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingCompromissoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AtualizarParcial(Guid id, [FromBody] UpdateStagingCompromissoRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(ApiResponse<object>.Fail("Payload inválido.", statusCode: 400));

        var item = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail("Registro pendente não encontrado.", statusCode: 404));

        if (request.TipoCompromisso is not null)
            item.TipoCompromisso = request.TipoCompromisso;

        if (request.Data.HasValue)
            item.Data = request.Data;

        if (request.Hora.HasValue)
            item.Hora = request.Hora;

        if (request.Local is not null)
            item.Local = request.Local;

        if (request.LembreteMinutos.HasValue)
            item.LembreteMinutos = request.LembreteMinutos;

        await _stagingRepo.UpdateAsync(item, ct);
        return Ok(ApiResponse<StagingCompromissoResponse>.Ok(Map(item)));
    }

    // ── POST /api/compromisso/staging/{id}/confirmar ─────────────────────
    // Dashboard: promove staging → compromisso (a ser implementado pelo @dev quando
    // o fluxo de criação de Compromisso estiver definido).

    [HttpPost("{id:guid}/confirmar")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Confirmar(Guid id, [FromBody] ConfirmarStagingCompromissoRequest? request, CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        // Validações mínimas (campos que o bot/triagem devem preencher)
        if (string.IsNullOrWhiteSpace(staging.TipoCompromisso))
            return BadRequest(ApiResponse<object>.Fail("Tipo de compromisso inválido.", statusCode: 400));

        if (staging.Data is null || !staging.Hora.HasValue)
            return BadRequest(ApiResponse<object>.Fail("Data e hora são obrigatórias para confirmar.", statusCode: 400));

        // Resolução do processo / cliente (quando bot enviou num_processo/cliente_nome).
        Guid? idProcesso = request?.IdProcesso;
        Guid? idCliente  = request?.IdCliente;

        if (idProcesso is null && !string.IsNullOrWhiteSpace(staging.NumProcesso))
        {
            var numProcessoCanonical = NormalizeNumProcesso(staging.NumProcesso);
            var proc = await _processoRepo.GetByNumeroAsync(numProcessoCanonical, ct);
            if (proc is not null)
            {
                idProcesso = proc.Id;
                idCliente = proc.ClienteId;
            }
        }

        if (idCliente is null && !string.IsNullOrWhiteSpace(staging.ClienteNome))
        {
            var cpf = Agile360.Shared.DocumentSanitizer.Sanitize(staging.ClienteNome);
            if (!string.IsNullOrWhiteSpace(cpf) && cpf!.Length == 11)
            {
                var cliente = await _clienteRepo.GetByCpfAsync(cpf, ct);
                if (cliente is not null)
                    idCliente = cliente.Id;
            }
        }

        // Compromisso 'Audiência' exige processo no modelo de produção.
        if (staging.TipoCompromisso == "Audiência" && idProcesso is null)
            return BadRequest(ApiResponse<object>.Fail(
                "Para 'Audiência', informe um processo válido para vincular (num_processo).",
                statusCode: 400));

        // Cria entidade de produção (validações adicionais do controller de produção ficam fora,
        // então garantimos pelo menos valores obrigatórios e enums válidos via lógica simples).
        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            AdvogadoId = _currentUser.AdvogadoId,
            IsActive = true,
            TipoCompromisso = staging.TipoCompromisso,
            TipoAudiencia = staging.TipoAudiencia,
            Data = staging.Data.Value,
            Hora = staging.Hora!.Value,
            Local = staging.Local,
            ClienteId = idCliente,
            ProcessoId = idProcesso,
            Observacoes = staging.Observacoes,
            LembreteMinutos = staging.LembreteMinutos,
        };

        await _compromissoRepo.AddAsync(compromisso, ct);

        var confirmed = await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, compromisso.Id, ct);
        if (!confirmed)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        return Ok(ApiResponse<object>.Ok(new { mensagem = "Compromisso confirmado com sucesso.", compromisso_id = compromisso.Id }));
    }

    // ── Normalização CNJ (mesmo padrão do staging_processo) ────────────────
    // CNJ (antigo) esperado: NNNNNNN-DD.AAAA.J.T.OOOO → 20 dígitos.
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

    // ── DELETE /api/compromisso/staging/{id} ─────────────────────────────

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
