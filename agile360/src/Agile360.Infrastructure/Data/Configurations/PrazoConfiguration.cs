using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class PrazoConfiguration : IEntityTypeConfiguration<Prazo>
{
    public void Configure(EntityTypeBuilder<Prazo> builder)
    {
        builder.ToTable("prazos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Descricao).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Prioridade).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.OrigemIntimacao).HasMaxLength(200);
        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => e.DataVencimento);
        builder.HasIndex(e => new { e.AdvogadoId, e.DataVencimento, e.Status });
        builder.Property<Guid?>("CreatedBy");
        builder.Property<Guid?>("LastModifiedBy");
    }
}
