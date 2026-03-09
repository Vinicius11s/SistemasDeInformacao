namespace Agile360.Domain.Entities;

/// <summary>
/// Chave de integração M2M para n8n, Evolution API, etc.
/// Apenas o SHA-256 hash da chave bruta é armazenado —
/// o valor real é exibido uma única vez no momento da criação.
/// </summary>
public class ApiKey
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    /// <summary>Nome amigável: "n8n Principal", "WhatsApp Bot". Coluna: nome_dispositivo.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>SHA-256 hex da chave bruta. Nunca exposto.</summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>Primeiros 8–12 caracteres da chave bruta (identificação segura).</summary>
    public string KeyPrefix { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Calculado — não mapeado para coluna.</summary>
    public bool IsActive =>
        Ativa &&
        RevokedAt == null &&
        (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);
}
