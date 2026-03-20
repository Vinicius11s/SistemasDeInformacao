using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Tabela: staging_prazo — fila de aprovação para criação de prazos.
/// Mantém colunas com nomes equivalentes às de produção (snake_case),
/// adicionando também o controle de staging via Status/ExpiresAt.
/// </summary>
public class StagingPrazo
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    // ── Campos (equivalentes aos da tabela Prazo) ─────────────────────────
    public Guid? ProcessoId { get; set; }
    public Guid? ClienteId { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? TipoPrazo { get; set; }
    public string? Prioridade { get; set; }
    public DateOnly? DataVencimento { get; set; }
    public DateOnly? DataPublicacao { get; set; }
    public string? TipoContagem { get; set; }
    public int? PrazoDias { get; set; }
    public bool SuspensaoPrazos { get; set; }

    // ── Controle de staging ───────────────────────────────────────────────
    public StagingStatus Status { get; set; } = StagingStatus.Pendente;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmadoEm { get; set; }
    public DateTimeOffset? RejeitadoEm { get; set; }

    /// <summary>Preenchido após confirmação — referencia o Prazo.Id criado.</summary>
    public Guid? PrazoIdGerado { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
}

