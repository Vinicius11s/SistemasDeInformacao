using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Representa um compromisso enviado pelo bot WhatsApp (n8n) aguardando
/// revisão e confirmação do advogado no Dashboard.
///
/// Não herda BaseEntity — registros de staging não estão sujeitos ao
/// HasQueryFilter de tenant; o repositório aplica isolamento por AdvogadoId
/// explicitamente em todos os métodos.
/// </summary>
public class StagingCompromisso
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    // ── Dados do compromisso — mesmos campos de Compromisso ──────────────
    public string? TipoCompromisso { get; set; }
    public string? TipoAudiencia { get; set; }
    public DateOnly? Data { get; set; }
    public TimeOnly? Hora { get; set; }
    public string? Local { get; set; }

    /// <summary>Nome ou CPF do cliente — o bot pode não conhecer o Guid ainda.</summary>
    public string? ClienteNome { get; set; }

    /// <summary>Número do processo — referência textual antes do vínculo com Guid.</summary>
    public string? NumProcesso { get; set; }

    public string? Observacoes { get; set; }
    public int? LembreteMinutos { get; set; }

    /// <summary>Texto bruto original da mensagem do WhatsApp — contexto para o advogado revisar.</summary>
    public string? OrigemMensagem { get; set; }

    // ── Controle de staging ──────────────────────────────────────────────
    public StagingStatus Status { get; set; } = StagingStatus.Pendente;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmadoEm { get; set; }
    public DateTimeOffset? RejeitadoEm { get; set; }

    /// <summary>Preenchido após confirmação — referencia o Compromisso.Id criado.</summary>
    public Guid? CompromissoIdGerado { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
}
