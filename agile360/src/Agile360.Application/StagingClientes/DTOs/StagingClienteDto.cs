using Agile360.Domain.Enums;

namespace Agile360.Application.StagingClientes.DTOs;

/// <summary>
/// Response DTO for a staging record sent to the dashboard for review.
/// </summary>
public record StagingClienteResponse(
    Guid Id,
    TipoPessoa TipoPessoa,
    string? Nome,
    string? CPF,
    string? RG,
    string? OrgaoExpedidor,
    string? RazaoSocial,
    string? CNPJ,
    string? InscricaoEstadual,
    string? Email,
    string? Telefone,
    string? WhatsAppNumero,
    DateOnly? DataReferencia,
    string? AreaAtuacao,
    string? Endereco,
    string? Observacoes,
    string Origem,
    string? OrigemMensagem,
    string Status,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Request DTO used by the n8n bot to submit a pending client registration.
/// All fields are optional — the bot may collect partial data.
/// </summary>
public record CreateStagingClienteRequest(
    TipoPessoa TipoPessoa,
    string? Nome,
    string? CPF,
    string? RG,
    string? OrgaoExpedidor,
    string? RazaoSocial,
    string? CNPJ,
    string? InscricaoEstadual,
    string? Email,
    string? Telefone,
    string? WhatsAppNumero,
    DateOnly? DataReferencia,
    string? AreaAtuacao,
    string? Endereco,
    string? Observacoes,
    // Raw WhatsApp message text — context for the advogado when reviewing.
    string? OrigemMensagem
);

/// <summary>Lightweight summary used for the dashboard badge / card.</summary>
public record StagingCountResponse(int Pendentes);
