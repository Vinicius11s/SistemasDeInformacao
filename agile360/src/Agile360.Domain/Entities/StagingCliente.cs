using Agile360.Domain.Enums;

namespace Agile360.Domain.Entities;

/// <summary>
/// Represents a client registration submitted by the WhatsApp bot (n8n) awaiting
/// the advogado's review and confirmation in the dashboard.
///
/// Does NOT inherit BaseEntity — staging records are NOT subject to the tenant
/// HasQueryFilter; the repository enforces AdvogadoId isolation explicitly.
/// </summary>
public class StagingCliente
{
    public Guid Id { get; set; }
    public Guid AdvogadoId { get; set; }

    // Client data — same fields as Cliente
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.PessoaFisica;
    public string? Nome { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public string? OrgaoExpedidor { get; set; }
    public string? RazaoSocial { get; set; }
    public string? CNPJ { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsAppNumero { get; set; }
    public DateOnly? DataReferencia { get; set; }
    public string? AreaAtuacao { get; set; }
    public string? Endereco { get; set; }
    public string? Observacoes { get; set; }
    public OrigemCliente Origem { get; set; } = OrigemCliente.WhatsApp;

    /// <summary>Raw WhatsApp message text — gives the advogado context when reviewing.</summary>
    public string? OrigemMensagem { get; set; }

    // Staging control
    public StagingStatus Status { get; set; } = StagingStatus.Pendente;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmadoEm { get; set; }
    public DateTimeOffset? RejeitadoEm { get; set; }

    /// <summary>Filled in after confirmation — references the created Cliente.Id.</summary>
    public Guid? ClienteIdGerado { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
}
