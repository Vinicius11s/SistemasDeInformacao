using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class PrazoConfiguration : IEntityTypeConfiguration<Prazo>
{
    public void Configure(EntityTypeBuilder<Prazo> builder)
    {
        builder.ToTable("prazo");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");
        builder.Property(e => e.ProcessoId).HasColumnName("id_processo");
        builder.Property(e => e.ClienteId) .HasColumnName("id_cliente");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.Titulo)    .IsRequired().HasMaxLength(300);
        builder.Property(e => e.TipoPrazo) .HasMaxLength(30);
        builder.Property(e => e.Prioridade).HasMaxLength(20);
        builder.Property(e => e.Status)    .HasMaxLength(20);
        builder.Property(e => e.TipoContagem).HasMaxLength(20);

        // Campos de BaseEntity sem coluna no banco
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        // Alias interno sem coluna
        builder.Ignore(e => e.Descricao_);

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => e.DataVencimento);
        builder.HasIndex(e => new { e.AdvogadoId, e.DataVencimento, e.Status });
    }
}
