using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class StagingProcessoConfiguration : IEntityTypeConfiguration<StagingProcesso>
{
    public void Configure(EntityTypeBuilder<StagingProcesso> builder)
    {
        builder.ToTable("staging_processo");
        builder.HasKey(e => e.Id);

        // FK: mesmo padrão das tabelas de produção — prefixo id_
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — UseSnakeCaseNamingConvention gera nomes das demais colunas
        builder.Property(e => e.NumProcesso)       .HasMaxLength(50);
        builder.Property(e => e.ParteContraria)    .HasMaxLength(200);
        builder.Property(e => e.Tribunal)          .HasMaxLength(200);
        builder.Property(e => e.ComarcaVara)       .HasMaxLength(200);
        builder.Property(e => e.Assunto)           .HasMaxLength(200);
        builder.Property(e => e.FaseProcessual)    .HasMaxLength(100);
        builder.Property(e => e.StatusProcesso)    .HasMaxLength(50);
        builder.Property(e => e.ClienteNome)       .HasMaxLength(200);
        builder.Property(e => e.ValorCausa)        .HasColumnType("numeric(18,2)");
        builder.Property(e => e.HonorariosEstimados).HasColumnType("numeric(18,2)");
        builder.Property(e => e.Status)            .HasConversion<string>().HasMaxLength(20).IsRequired();

        // Propriedade computada — não mapeada
        builder.Ignore(e => e.IsExpired);

        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        builder.HasIndex(e => e.ExpiresAt);
    }
}
