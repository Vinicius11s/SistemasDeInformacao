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
    string? OAB,
    string? NomeEscritorio,
    string? Plano,
    string? StatusAssinatura,
    string? FotoUrl,
    string? Telefone,
    string? Cidade,
    string? Estado,
    DateOnly? DataExpiracao);      // date no Postgres → DateOnly no C# (sem timezone)
