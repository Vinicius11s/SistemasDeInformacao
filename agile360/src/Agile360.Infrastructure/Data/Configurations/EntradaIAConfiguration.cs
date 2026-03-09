using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class EntradaIAConfiguration : IEntityTypeConfiguration<EntradaIA>
{
    public void Configure(EntityTypeBuilder<EntradaIA> builder)
    {
        builder.ToTable("entrada_ia");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");
        builder.Property(e => e.ClienteId) .HasColumnName("id_cliente");
        builder.Property(e => e.ProcessoId).HasColumnName("id_processo");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.ConteudoOriginal).IsRequired();
        builder.Property(e => e.Origem)          .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status)          .HasConversion<string>().HasMaxLength(20);

        // Campos de BaseEntity sem coluna no banco
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        builder.HasIndex(e => e.AdvogadoId);
    }
}
