using Agile360.API.Models;
using Agile360.Application.Clientes.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingClientes.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

/// <summary>
/// Manages the WhatsApp bot approval queue.
///
/// POST   /api/clientes/staging            → n8n (API Key): save pending record
/// GET    /api/clientes/staging            → dashboard (JWT): list pending records
/// GET    /api/clientes/staging/count      → dashboard (JWT): badge count
/// POST   /api/clientes/staging/{id}/confirmar → dashboard (JWT): promote to clientes
/// DELETE /api/clientes/staging/{id}       → dashboard (JWT): reject/discard
/// </summary>
[ApiController]
[Route("api/clientes/staging")]
[Authorize]
public class StagingClienteController : ControllerBase
{
    private readonly IStagingClienteRepository _stagingRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public StagingClienteController(
        IStagingClienteRepository stagingRepo,
        IClienteRepository clienteRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _stagingRepo  = stagingRepo;
        _clienteRepo  = clienteRepo;
        _uow          = uow;
        _currentUser  = currentUser;
    }

    // ── POST /api/clientes/staging ────────────────────────────────────────
    // Called by the n8n bot (API Key). Creates a Pendente record.

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StagingClienteResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStagingClienteRequest request,
        CancellationToken ct)
    {
        // Basic guard: must have at least a name or razão social
        if (string.IsNullOrWhiteSpace(request.Nome) &&
            string.IsNullOrWhiteSpace(request.RazaoSocial))
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe pelo menos o Nome (PF) ou Razão Social (PJ).", statusCode: 400));
        }

        var item = new StagingCliente
        {
            Id              = Guid.NewGuid(),
            AdvogadoId      = _currentUser.AdvogadoId,
            TipoPessoa      = request.TipoPessoa,
            Nome            = request.Nome,
            CPF             = DocumentSanitizer.Sanitize(request.CPF),
            RG              = DocumentSanitizer.Sanitize(request.RG),
            OrgaoExpedidor  = request.OrgaoExpedidor,
            RazaoSocial     = request.RazaoSocial,
            CNPJ            = DocumentSanitizer.Sanitize(request.CNPJ),
            InscricaoEstadual = request.InscricaoEstadual,
            Email           = request.Email,
            Telefone        = DocumentSanitizer.Sanitize(request.Telefone),
            WhatsAppNumero  = DocumentSanitizer.Sanitize(request.WhatsAppNumero),
            DataReferencia  = request.DataReferencia,
            AreaAtuacao     = request.AreaAtuacao,
            Endereco        = request.Endereco,
            Observacoes     = request.Observacoes,
            Origem          = OrigemCliente.WhatsApp,
            OrigemMensagem  = request.OrigemMensagem,
            Status          = StagingStatus.Pendente,
            ExpiresAt       = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow,
        };

        await _stagingRepo.CreateAsync(item, ct);
        return StatusCode(201, ApiResponse<StagingClienteResponse>.Ok(Map(item)));
    }

    // ── GET /api/clientes/staging ─────────────────────────────────────────
    // Dashboard: list all Pendente records for the logged-in advogado.

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingClienteResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingClienteResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/clientes/staging/count ───────────────────────────────────
    // Dashboard badge: lightweight count endpoint.

    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<StagingCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<StagingCountResponse>.Ok(new StagingCountResponse(count)));
    }

    // ── POST /api/clientes/staging/{id}/confirmar ─────────────────────────
    // Dashboard: promote staging record → clientes (with full sanitisation & dedup).

    [HttpPost("{id:guid}/confirmar")]
    [ProducesResponseType(typeof(ApiResponse<ClienteResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    {
        var staging = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (staging is null || staging.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail(
                "Registro pendente não encontrado ou já processado.", statusCode: 404));

        // Deduplication: prevent duplicate CPF in production table
        if (!string.IsNullOrEmpty(staging.CPF))
        {
            var dup = await _clienteRepo.GetByCpfAsync(staging.CPF, ct);
            if (dup != null)
                return Conflict(ApiResponse<object>.Fail(
                    "Já existe um cliente com este CPF na sua base.", statusCode: 409));
        }

        // Create the confirmed cliente in production
        var cliente = new Cliente
        {
            Id                = Guid.NewGuid(),
            AdvogadoId        = _currentUser.AdvogadoId,
            TipoCliente       = staging.TipoPessoa == Agile360.Domain.Enums.TipoPessoa.PessoaJuridica
                                    ? "Pessoa Jurídica" : "Pessoa Física",
            NomeCompleto      = staging.Nome,
            CPF               = staging.CPF,
            RG                = staging.RG,
            OrgaoExpedidor    = staging.OrgaoExpedidor,
            RazaoSocial       = staging.RazaoSocial,
            CNPJ              = staging.CNPJ,
            InscricaoEstadual = staging.InscricaoEstadual,
            Telefone          = staging.Telefone,
            DataReferencia    = staging.DataReferencia,
            AreaAtuacao       = staging.AreaAtuacao,
            Endereco          = staging.Endereco,
            Observacoes       = staging.Observacoes,
            IsActive          = true,
        };

        await _clienteRepo.AddAsync(cliente, ct);
        await _uow.SaveChangesAsync(ct);

        // Mark staging record as Confirmado
        await _stagingRepo.ConfirmarAsync(id, _currentUser.AdvogadoId, cliente.Id, ct);

        return Ok(ApiResponse<ClienteResponse>.Ok(MapCliente(cliente)));
    }

    // ── DELETE /api/clientes/staging/{id} ────────────────────────────────
    // Dashboard: reject / discard staging record.

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

    // ── Private mappers ───────────────────────────────────────────────────

    private static StagingClienteResponse Map(StagingCliente s) => new(
        s.Id,
        s.TipoPessoa,
        s.Nome, s.CPF, s.RG, s.OrgaoExpedidor,
        s.RazaoSocial, s.CNPJ, s.InscricaoEstadual,
        s.Email, s.Telefone, s.WhatsAppNumero,
        s.DataReferencia, s.AreaAtuacao,
        s.Endereco, s.Observacoes,
        s.Origem.ToString(), s.OrigemMensagem,
        s.Status.ToString(),
        s.ExpiresAt,
        s.CreatedAt);

    private static ClienteResponse MapCliente(Cliente c) => new(
        Id:               c.Id,
        TipoCliente:      c.TipoCliente,
        NomeCompleto:     c.NomeCompleto,
        CPF:              c.CPF,
        RG:               c.RG,
        OrgaoExpedidor:   c.OrgaoExpedidor,
        RazaoSocial:      c.RazaoSocial,
        CNPJ:             c.CNPJ,
        InscricaoEstadual: c.InscricaoEstadual,
        Telefone:         c.Telefone,
        CEP:              c.CEP,
        Estado:           c.Estado,
        Cidade:           c.Cidade,
        Endereco:         c.Endereco,
        Numero:           c.Numero,
        Bairro:           c.Bairro,
        Complemento:      c.Complemento,
        DataReferencia:   c.DataReferencia,
        EstadoCivil:      c.EstadoCivil,
        AreaAtuacao:      c.AreaAtuacao,
        NumeroConta:      c.NumeroConta,
        Pix:              c.Pix,
        IsActive:         c.IsActive,
        Observacoes:      c.Observacoes,
        DataCadastro:     c.DataCadastro);
}
