using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class StagingCompromissoConfiguration : IEntityTypeConfiguration<StagingCompromisso>
{
    public void Configure(EntityTypeBuilder<StagingCompromisso> builder)
    {
        builder.ToTable("staging_compromisso");
        builder.HasKey(e => e.Id);

        // FK: mesmo padrão das tabelas de produção — prefixo id_
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — UseSnakeCaseNamingConvention gera nomes das demais colunas
        builder.Property(e => e.TipoCompromisso).HasMaxLength(100);
        builder.Property(e => e.TipoAudiencia)  .HasMaxLength(100);
        builder.Property(e => e.Local)          .HasMaxLength(500);
        builder.Property(e => e.ClienteNome)    .HasMaxLength(200);
        builder.Property(e => e.NumProcesso)    .HasMaxLength(50);
        builder.Property(e => e.Status)         .HasConversion<string>().HasMaxLength(20).IsRequired();

        // Propriedade computada — não mapeada
        builder.Ignore(e => e.IsExpired);

        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        builder.HasIndex(e => e.ExpiresAt);
    }
}
