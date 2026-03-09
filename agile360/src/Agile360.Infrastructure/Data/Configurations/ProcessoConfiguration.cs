using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class ProcessoConfiguration : IEntityTypeConfiguration<Processo>
{
    public void Configure(EntityTypeBuilder<Processo> builder)
    {
        builder.ToTable("processo");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");
        builder.Property(e => e.ClienteId) .HasColumnName("id_cliente");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.NumProcesso)   .HasMaxLength(50);
        builder.Property(e => e.Tribunal)      .HasMaxLength(100);
        builder.Property(e => e.ComarcaVara)   .HasMaxLength(150);
        builder.Property(e => e.FaseProcessual).HasMaxLength(50);
        builder.Property(e => e.Status)        .HasMaxLength(20);

        // Campos de BaseEntity sem coluna no banco
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        // Propriedades calculadas (aliases sem coluna)
        builder.Ignore(e => e.Comarca);
        builder.Ignore(e => e.TipoAcao);
        builder.Ignore(e => e.NumeroProcesso);

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => new { e.AdvogadoId, e.NumProcesso }).IsUnique();
    }
}
