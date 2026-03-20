using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class StagingClienteConfiguration : IEntityTypeConfiguration<StagingCliente>
{
    public void Configure(EntityTypeBuilder<StagingCliente> builder)
    {
        builder.ToTable("staging_cliente");
        builder.HasKey(e => e.Id);

        // FK: banco já existente usa coluna "id_advogado"
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.TipoPessoa)        .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.NomeCompleto)      .HasMaxLength(300);
        builder.Property(e => e.CPF)               .HasMaxLength(14);
        builder.Property(e => e.RG)                .HasMaxLength(20);
        builder.Property(e => e.OrgaoExpedidor)    .HasMaxLength(20);
        builder.Property(e => e.RazaoSocial)       .HasMaxLength(300);
        builder.Property(e => e.CNPJ)              .HasMaxLength(18);
        builder.Property(e => e.InscricaoEstadual) .HasMaxLength(20);
        builder.Property(e => e.Email)             .HasMaxLength(256);
        builder.Property(e => e.Telefone)          .HasMaxLength(20);
        builder.Property(e => e.CEP)               .HasMaxLength(9);
        builder.Property(e => e.Estado)            .HasMaxLength(2);
        builder.Property(e => e.Cidade)            .HasMaxLength(100);
        builder.Property(e => e.Endereco)          .HasMaxLength(300);
        builder.Property(e => e.Numero)            .HasMaxLength(20);
        builder.Property(e => e.Bairro)            .HasMaxLength(100);
        builder.Property(e => e.Complemento)       .HasMaxLength(100);
        builder.Property(e => e.EstadoCivil)       .HasMaxLength(20);
        builder.Property(e => e.AreaAtuacao)       .HasMaxLength(200);
        builder.Property(e => e.NumeroConta)       .HasMaxLength(50);
        builder.Property(e => e.Pix)               .HasMaxLength(100);
        // A convenção geraria "whats_app_numero" por causa de "WhatsApp",
        // mas o schema do Postgres usa "whatsapp_numero".
        builder.Property(e => e.WhatsAppNumero)
            .HasColumnName("whatsapp_numero")
            .HasMaxLength(20);
        builder.Property(e => e.Origem)            .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status)            .HasConversion<string>().HasMaxLength(20).IsRequired();

        // Computed property — not mapped
        builder.Ignore(e => e.IsExpired);

        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        // A query do dashboard filtra por id_advogado + status + expires_at.
        // Índice composto melhora o COUNT e reduz chance de varredura completa.
        builder.HasIndex(e => new { e.AdvogadoId, e.Status, e.ExpiresAt });
        builder.HasIndex(e => e.ExpiresAt);
    }
}
