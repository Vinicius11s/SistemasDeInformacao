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

        // FK: banco usa id_ prefix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.TipoPessoa)       .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Nome)             .HasMaxLength(200);
        builder.Property(e => e.CPF)              .HasMaxLength(14);
        builder.Property(e => e.RG)               .HasMaxLength(20);
        builder.Property(e => e.OrgaoExpedidor)   .HasMaxLength(20);
        builder.Property(e => e.RazaoSocial)      .HasMaxLength(200);
        builder.Property(e => e.CNPJ)             .HasMaxLength(18);
        builder.Property(e => e.InscricaoEstadual).HasMaxLength(20);
        builder.Property(e => e.Email)            .HasMaxLength(256);
        builder.Property(e => e.Telefone)         .HasMaxLength(20);
        builder.Property(e => e.WhatsAppNumero)   .HasMaxLength(20);
        builder.Property(e => e.AreaAtuacao)      .HasMaxLength(200);
        builder.Property(e => e.Endereco)         .HasMaxLength(500);
        builder.Property(e => e.Origem)           .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status)           .HasConversion<string>().HasMaxLength(20).IsRequired();

        // Computed property — not mapped
        builder.Ignore(e => e.IsExpired);

        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        builder.HasIndex(e => e.ExpiresAt);
    }
}
