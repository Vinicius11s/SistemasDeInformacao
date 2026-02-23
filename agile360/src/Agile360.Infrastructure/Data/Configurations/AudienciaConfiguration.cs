using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class AudienciaConfiguration : IEntityTypeConfiguration<Audiencia>
{
    public void Configure(EntityTypeBuilder<Audiencia> builder)
    {
        builder.ToTable("audiencias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Local).HasMaxLength(300);
        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Observacoes);
        builder.Property(e => e.GoogleEventId).HasMaxLength(100);
        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => e.DataHora);
        builder.HasIndex(e => new { e.AdvogadoId, e.DataHora, e.Status });
        builder.Property<Guid?>("CreatedBy");
        builder.Property<Guid?>("LastModifiedBy");
    }
}
