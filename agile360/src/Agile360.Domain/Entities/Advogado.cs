namespace Agile360.Domain.Entities;

/// <summary>
/// Tenant root entity. Each advogado is a tenant; does not have AdvogadoId.
/// </summary>
public class Advogado
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OAB { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? WhatsAppId { get; set; }
    public string? FotoUrl { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    // PasswordHash used for local authentication (PBKDF2). Nullable for legacy users.
    public string? PasswordHash { get; set; }
}
