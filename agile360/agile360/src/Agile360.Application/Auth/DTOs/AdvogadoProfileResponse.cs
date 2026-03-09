namespace Agile360.Application.Auth.DTOs;

/// <summary>
/// Dados do perfil do advogado retornados ao cliente após login/registro.
/// Inclui apenas os campos necessários para a UI — sem dados sensíveis.
/// </summary>
public record AdvogadoProfileResponse(
    Guid    Id,
    string  Nome,
    string  Email,
    string? Role,
    string? NumeroOab,
    string? OabUf,
    string? NomeEscritorio,
    string? Plano,
    string? StatusAssinatura,
    string? LogoUrl,
    string? TelefoneContato,
    string? Cidade,
    string? Estado,
    DateTimeOffset? DataExpiracao);
