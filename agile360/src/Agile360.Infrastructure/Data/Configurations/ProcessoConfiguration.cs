using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class ProcessoConfiguration : IEntityTypeConfiguration<Processo>
{
    public void Configure(EntityTypeBuilder<Processo> builder)
    {
        builder.ToTable("processos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NumeroProcesso).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Vara).HasMaxLength(100);
        builder.Property(e => e.Comarca).HasMaxLength(100);
        builder.Property(e => e.Tribunal).HasMaxLength(100);
        builder.Property(e => e.TipoAcao).HasMaxLength(200);
        builder.Property(e => e.Descricao);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => new { e.AdvogadoId, e.NumeroProcesso }).IsUnique();
        builder.HasIndex(e => new { e.AdvogadoId, e.Status });
        builder.Property<Guid?>("CreatedBy");
        builder.Property<Guid?>("LastModifiedBy");
    }
}
