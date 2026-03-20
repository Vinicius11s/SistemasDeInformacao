using Agile360.API.Models;
using Agile360.Application.Clientes.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Application.StagingClientes.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Shared;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Agile360.API.Controllers;

/// <summary>
/// Manages the WhatsApp bot approval queue.
///
/// POST   /api/cliente/staging            → n8n (API Key): save pending record
/// GET    /api/cliente/staging            → dashboard (JWT): list pending records
/// GET    /api/cliente/staging/count      → dashboard (JWT): badge count
/// POST   /api/cliente/staging/{id}/confirmar → dashboard (JWT): promote to clientes
/// DELETE /api/cliente/staging/{id}       → dashboard (JWT): reject/discard
/// </summary>
[ApiController]
[Route("api/cliente/staging")]
public class StagingClienteController : ControllerBase
{
    private readonly IStagingClienteRepository _stagingRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<StagingClienteController> _logger;

    public StagingClienteController(
        IStagingClienteRepository stagingRepo,
        IClienteRepository clienteRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<StagingClienteController> logger)
    {
        _stagingRepo  = stagingRepo;
        _clienteRepo  = clienteRepo;
        _uow          = uow;
        _currentUser  = currentUser;
        _logger       = logger;
    }

    // ── POST /api/cliente/staging ────────────────────────────────────────
    // Called by the n8n bot (API Key). Creates a Pendente record.
    // Uses "JwtOrApiKey" policy so the n8n bot can authenticate with X-Api-Key
    // instead of a JWT Bearer token.

    [HttpPost]
    [Authorize(Policy = "JwtOrApiKey")]
    [EnableCors("ApiIntegration")]
    [ProducesResponseType(typeof(ApiResponse<StagingClienteResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStagingClienteRequest request,
        CancellationToken ct)
    {
        // Basic guard: must have at least a nome completo or razão social
        if (string.IsNullOrWhiteSpace(request.NomeCompleto) &&
            string.IsNullOrWhiteSpace(request.RazaoSocial))
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Informe pelo menos o Nome (PF) ou Razão Social (PJ).", statusCode: 400));
        }

        // TipoPessoa é derivado do texto de tipo_cliente enviado pelo n8n
        var tipoCliente = (request.TipoCliente ?? "Pessoa Física").Trim();
        var tipoPessoa = tipoCliente.Equals("Pessoa Jurídica", StringComparison.OrdinalIgnoreCase)
            ? TipoPessoa.PessoaJuridica
            : TipoPessoa.PessoaFisica;

        var item = new StagingCliente
        {
            Id              = Guid.NewGuid(),
            AdvogadoId      = _currentUser.AdvogadoId,
            TipoPessoa      = tipoPessoa,
            NomeCompleto    = request.NomeCompleto,
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
            CEP             = DocumentSanitizer.Sanitize(request.CEP),
            Estado          = request.Estado,
            Cidade          = request.Cidade,
            AreaAtuacao     = request.AreaAtuacao,
            Endereco        = request.Endereco,
            Numero          = request.Numero,
            Bairro          = request.Bairro,
            Complemento     = request.Complemento,
            EstadoCivil     = request.EstadoCivil,
            NumeroConta     = request.NumeroConta,
            Pix             = request.Pix,
            Observacoes     = request.Observacoes,
            Origem          = OrigemCliente.WhatsApp,
            OrigemMensagem  = request.Mensagem,
            Status          = StagingStatus.Pendente,
            ExpiresAt       = DateTimeOffset.UtcNow.AddHours(24),
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow,
        };

        _logger.LogInformation(
            "Autenticado Advogado {AdvogadoId} -> Salvando na Staging para este Tenant",
            item.AdvogadoId);

        await _stagingRepo.CreateAsync(item, ct);
        // Confirmação para auditoria: request chegou, API key autenticou (log no handler) e o insert foi realizado.
        var authMethod = HttpContext.User.FindFirst("auth_method")?.Value ?? "unknown";
        _logger.LogInformation("[STAGING_CLIENTE] Criado {StagingId} para AdvogadoId {AdvogadoId} via {AuthMethod}",
            item.Id, item.AdvogadoId, authMethod);
        return StatusCode(201, ApiResponse<StagingClienteResponse>.Ok(Map(item)));
    }

    // ── GET /api/cliente/staging ─────────────────────────────────────────
    // Dashboard: list all Pendente records for the logged-in advogado.

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StagingClienteResponse>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _stagingRepo.ListPendentesAsync(_currentUser.AdvogadoId, ct);
        return Ok(ApiResponse<IReadOnlyList<StagingClienteResponse>>.Ok(
            items.Select(Map).ToList()));
    }

    // ── GET /api/cliente/staging/count ───────────────────────────────────
    // Dashboard badge: lightweight count endpoint.

    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingCountResponse>), 200)]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var count = await _stagingRepo.CountPendentesAsync(_currentUser.AdvogadoId, ct);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning(
                "[StagingCliente] CountPendentes demorou {ElapsedMs}ms (tenant={AdvogadoId}).",
                sw.ElapsedMilliseconds, _currentUser.AdvogadoId);
        }
        return Ok(ApiResponse<StagingCountResponse>.Ok(new StagingCountResponse(count)));
    }

    // ── PATCH /api/cliente/staging/{id} ────────────────────────────────
    // Dashboard: permite ao advogado ajustar dados antes de confirmar.
    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StagingClienteResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AtualizarParcial(Guid id, [FromBody] UpdateStagingClienteRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(ApiResponse<object>.Fail("Payload inválido.", statusCode: 400));

        var item = await _stagingRepo.GetByIdAsync(id, _currentUser.AdvogadoId, ct);
        if (item is null || item.Status != StagingStatus.Pendente)
            return NotFound(ApiResponse<object>.Fail("Registro pendente não encontrado.", statusCode: 404));

        // Atualiza somente campos informados (null => não altera).
        if (request.NomeCompleto is not null)
            item.NomeCompleto = request.NomeCompleto;

        if (request.CPF is not null)
            item.CPF = DocumentSanitizer.Sanitize(request.CPF);

        if (request.RG is not null)
            item.RG = DocumentSanitizer.Sanitize(request.RG);

        if (request.OrgaoExpedidor is not null)
            item.OrgaoExpedidor = request.OrgaoExpedidor.Trim();

        if (request.Telefone is not null)
            item.Telefone = DocumentSanitizer.Sanitize(request.Telefone);

        if (request.WhatsAppNumero is not null)
            item.WhatsAppNumero = DocumentSanitizer.Sanitize(request.WhatsAppNumero);

        if (request.RazaoSocial is not null)
            item.RazaoSocial = request.RazaoSocial;

        if (request.CNPJ is not null)
            item.CNPJ = DocumentSanitizer.Sanitize(request.CNPJ);

        if (request.InscricaoEstadual is not null)
            item.InscricaoEstadual = request.InscricaoEstadual.Trim();

        if (request.Email is not null)
            item.Email = request.Email;

        if (request.DataReferencia is not null)
            item.DataReferencia = request.DataReferencia;

        if (request.EstadoCivil is not null)
            item.EstadoCivil = request.EstadoCivil;

        if (request.AreaAtuacao is not null)
            item.AreaAtuacao = request.AreaAtuacao;

        if (request.CEP is not null)
            item.CEP = DocumentSanitizer.Sanitize(request.CEP);

        if (request.Estado is not null)
            item.Estado = request.Estado;

        if (request.Cidade is not null)
            item.Cidade = request.Cidade;

        if (request.Endereco is not null)
            item.Endereco = request.Endereco;

        if (request.Numero is not null)
            item.Numero = request.Numero;

        if (request.Bairro is not null)
            item.Bairro = request.Bairro;

        if (request.Complemento is not null)
            item.Complemento = request.Complemento;

        if (request.NumeroConta is not null)
            item.NumeroConta = request.NumeroConta.Trim();

        if (request.Pix is not null)
            item.Pix = request.Pix;

        if (request.Observacoes is not null)
            item.Observacoes = request.Observacoes;

        await _stagingRepo.UpdateAsync(item, ct);

        return Ok(ApiResponse<StagingClienteResponse>.Ok(Map(item)));
    }

    // ── POST /api/cliente/staging/{id}/confirmar ─────────────────────────
    // Dashboard: promote staging record → clientes (with full sanitisation & dedup).

    [HttpPost("{id:guid}/confirmar")]
    [Authorize]
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
            NomeCompleto      = staging.NomeCompleto,
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

    // ── DELETE /api/cliente/staging/{id} ────────────────────────────────
    // Dashboard: reject / discard staging record.

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

    // ── Private mappers ───────────────────────────────────────────────────

    private static StagingClienteResponse Map(StagingCliente s) => new(
        Id:              s.Id,
        TipoPessoa:     s.TipoPessoa,
        Nome:            s.NomeCompleto,
        CPF:             s.CPF,
        RG:              s.RG,
        OrgaoExpedidor:  s.OrgaoExpedidor,
        RazaoSocial:     s.RazaoSocial,
        CNPJ:            s.CNPJ,
        InscricaoEstadual: s.InscricaoEstadual,
        Email:           s.Email,
        Telefone:        s.Telefone,
        WhatsAppNumero:  s.WhatsAppNumero,
        DataReferencia:  s.DataReferencia,
        AreaAtuacao:     s.AreaAtuacao,
        CEP:             s.CEP,
        Estado:         s.Estado,
        Cidade:         s.Cidade,
        Endereco:       s.Endereco,
        Numero:         s.Numero,
        Bairro:         s.Bairro,
        Complemento:    s.Complemento,
        EstadoCivil:    s.EstadoCivil,
        NumeroConta:    s.NumeroConta,
        Pix:            s.Pix,
        Observacoes:    s.Observacoes,
        Origem:         s.Origem.ToString(),
        OrigemMensagem: s.OrigemMensagem,
        Status:         s.Status.ToString(),
        ExpiresAt:      s.ExpiresAt,
        CreatedAt:      s.CreatedAt);

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
