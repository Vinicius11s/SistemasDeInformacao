namespace Agile360.Domain.Entities;

/// <summary>
/// Representa um código de recuperação de emergência para MFA.
///
/// Política de segurança:
///   - <see cref="CodeHash"/> armazena APENAS o hash BCrypt (cost 12) — nunca o plaintext.
///   - <see cref="IsUsed"/> é marcado como true após o primeiro uso (burn-after-use).
///   - <see cref="UsedAt"/> registra o timestamp de uso para auditoria.
///   - Ao desativar o MFA, todos os registros deste advogado são deletados (hard delete).
/// </summary>
public class RecoveryCode
{
    /// <summary>Chave primária UUID gerada pelo banco.</summary>
    public Guid Id { get; set; }

    /// <summary>FK para <see cref="Advogado"/>. CASCADE DELETE garante limpeza automática.</summary>
    public Guid AdvogadoId { get; set; }

    /// <summary>Hash BCrypt (cost 12) do código de recuperação em texto limpo.</summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o código já foi utilizado (burn-after-use).
    /// Uma vez true, o código não pode ser aceito novamente.
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Timestamp UTC do momento em que o código foi utilizado. NULL enquanto não usado.</summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>Timestamp UTC de criação — auditoria completa.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ───────────────────────────────────────────────────────────────

    /// <summary>Navigation property para o advogado dono do código.</summary>
    public Advogado? Advogado { get; set; }
}
