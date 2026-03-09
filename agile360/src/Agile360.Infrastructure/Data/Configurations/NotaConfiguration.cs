using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class NotaConfiguration : IEntityTypeConfiguration<Nota>
{
    public void Configure(EntityTypeBuilder<Nota> builder)
    {
        builder.ToTable("nota");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");
        builder.Property(e => e.ProcessoId).HasColumnName("id_processo");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.Titulo)  .IsRequired().HasMaxLength(200);
        builder.Property(e => e.Conteudo).IsRequired();

        // Campos de BaseEntity sem coluna no banco
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        builder.HasIndex(e => e.AdvogadoId);
    }
}
