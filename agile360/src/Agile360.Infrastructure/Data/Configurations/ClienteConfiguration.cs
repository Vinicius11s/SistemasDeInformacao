using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CPF).HasMaxLength(14);
        builder.Property(e => e.RG).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Telefone).HasMaxLength(20);
        builder.Property(e => e.WhatsAppNumero).HasMaxLength(20);
        builder.Property(e => e.Endereco).HasMaxLength(500);
        builder.Property(e => e.Observacoes);
        builder.Property(e => e.Origem).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => new { e.AdvogadoId, e.CPF });
        builder.Property<Guid?>("CreatedBy");
        builder.Property<Guid?>("LastModifiedBy");
    }
}
