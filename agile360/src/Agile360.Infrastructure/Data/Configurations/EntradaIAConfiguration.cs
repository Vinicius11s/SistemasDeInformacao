using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class EntradaIAConfiguration : IEntityTypeConfiguration<EntradaIA>
{
    public void Configure(EntityTypeBuilder<EntradaIA> builder)
    {
        builder.ToTable("entradas_ia");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ConteudoOriginal).IsRequired();
        builder.Property(e => e.DadosExtraidos);
        builder.Property(e => e.Origem).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.AdvogadoId);
    }
}
