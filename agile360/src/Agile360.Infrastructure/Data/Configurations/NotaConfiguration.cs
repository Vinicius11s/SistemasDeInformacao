using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class NotaConfiguration : IEntityTypeConfiguration<Nota>
{
    public void Configure(EntityTypeBuilder<Nota> builder)
    {
        builder.ToTable("notas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Conteudo).IsRequired();
        builder.HasIndex(e => e.AdvogadoId);
        builder.Property<Guid?>("CreatedBy");
        builder.Property<Guid?>("LastModifiedBy");
    }
}
