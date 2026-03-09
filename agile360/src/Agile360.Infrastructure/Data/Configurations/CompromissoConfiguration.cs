using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class CompromissoConfiguration : IEntityTypeConfiguration<Compromisso>
{
    public void Configure(EntityTypeBuilder<Compromisso> builder)
    {
        builder.ToTable("compromisso");
        builder.HasKey(e => e.Id);

        // FK columns: banco usa id_ prefix, convenção geraria _id suffix — override necessário
        builder.Property(e => e.AdvogadoId) .HasColumnName("id_advogado");
        builder.Property(e => e.ClienteId)  .HasColumnName("id_cliente");
        builder.Property(e => e.ProcessoId) .HasColumnName("id_processo");

        // Constraints — nomes de coluna gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.TipoCompromisso).IsRequired().HasMaxLength(50);
        builder.Property(e => e.TipoAudiencia)  .HasMaxLength(50);
        builder.Property(e => e.Local)           .HasMaxLength(300);

        // Campos de BaseEntity sem coluna no banco
        // Atenção: IsActive NÃO existe na tabela compromisso (schema: id, tipo_compromisso, data,
        // hora, id_advogado, observacoes, status). A presença desta coluna causava o dashboard
        // ficar em branco porque a query falhava silenciosamente no try/catch do controller.
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => new { e.AdvogadoId, e.Data });
    }
}
