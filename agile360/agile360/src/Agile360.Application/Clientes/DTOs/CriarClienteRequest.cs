namespace Agile360.Application.Clientes.DTOs;

/// <summary>Payload para POST /api/clientes</summary>
public record CriarClienteRequest(
    // ── Identificação ──────────────────────────────────────────────────────────
    string  TipoCliente,      // 'Pessoa Física' | 'Pessoa Jurídica'
    string NomeCompleto,
    string? Cpf,
    string? Rg,
    string? OrgaoExpedidor,
    DateOnly? DataNascimento,
    string? EstadoCivil,
    string? Profissao,

    // ── Contato ────────────────────────────────────────────────────────────────
    string? Telefone,

    // ── Financeiro ─────────────────────────────────────────────────────────────
    string? NumeroConta,
    string? Pix,

    // ── Endereço ───────────────────────────────────────────────────────────────
    string? Cep,
    string? Endereco,
    string? Numero,
    string? Bairro,
    string? Complemento,
    string? Cidade,
    string? Estado          // max 2 chars — ex.: "SP"
);
