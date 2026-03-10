using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Representa um processo enviado pelo bot WhatsApp (n8n) aguardando
/// revisão e confirmação do advogado no Dashboard.
///
/// Não herda BaseEntity — registros de staging não estão sujeitos ao
/// HasQueryFilter de tenant; o repositório aplica isolamento por AdvogadoId
/// explicitamente em todos os métodos.
/// </summary>
public class StagingProcesso
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    // ── Dados do processo — mesmos campos de Processo ────────────────────
    public string? NumProcesso { get; set; }
    public string? ParteContraria { get; set; }
    public string? Tribunal { get; set; }
    public string? ComarcaVara { get; set; }
    public string? Assunto { get; set; }
    public decimal? ValorCausa { get; set; }
    public decimal? HonorariosEstimados { get; set; }
    public string? FaseProcessual { get; set; }
    public string? StatusProcesso { get; set; }
    public DateOnly? DataDistribuicao { get; set; }
    public string? Observacoes { get; set; }

    /// <summary>Nome ou CPF do cliente — o bot pode não conhecer o Guid ainda.</summary>
    public string? ClienteNome { get; set; }

    /// <summary>Texto bruto original da mensagem do WhatsApp — contexto para o advogado revisar.</summary>
    public string? OrigemMensagem { get; set; }

    // ── Controle de staging ──────────────────────────────────────────────
    public StagingStatus Status { get; set; } = StagingStatus.Pendente;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmadoEm { get; set; }
    public DateTimeOffset? RejeitadoEm { get; set; }

    /// <summary>Preenchido após confirmação — referencia o Processo.Id criado.</summary>
    public Guid? ProcessoIdGerado { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
}
