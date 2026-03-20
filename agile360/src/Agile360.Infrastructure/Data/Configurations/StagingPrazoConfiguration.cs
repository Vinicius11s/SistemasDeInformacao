using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class StagingPrazoConfiguration : IEntityTypeConfiguration<StagingPrazo>
{
    public void Configure(EntityTypeBuilder<StagingPrazo> builder)
    {
        builder.ToTable("staging_prazo");
        builder.HasKey(e => e.Id);

        // FK tenant
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // FK produção (nomes reais do schema: id_cliente / id_processo)
        builder.Property(e => e.ProcessoId).HasColumnName("id_processo");
        builder.Property(e => e.ClienteId) .HasColumnName("id_cliente");

        // Campos (snake_case via convenção + limites defensivos)
        builder.Property(e => e.Titulo).HasMaxLength(200);
        builder.Property(e => e.TipoPrazo).HasMaxLength(100);
        builder.Property(e => e.Prioridade).HasMaxLength(50);
        builder.Property(e => e.TipoContagem).HasMaxLength(20);

        // Staging status (Pendente/Confirmado/Rejeitado)
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Ignore(e => e.IsExpired);

        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        builder.HasIndex(e => e.ExpiresAt);
    }
}

