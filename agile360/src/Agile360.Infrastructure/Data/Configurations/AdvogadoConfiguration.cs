using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class AdvogadoConfiguration : IEntityTypeConfiguration<Advogado>
{
    public void Configure(EntityTypeBuilder<Advogado> builder)
    {
        // Map to existing table name in the database (singular 'advogado')
        builder.ToTable("advogado");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.OAB).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Telefone).HasMaxLength(20);
        builder.Property(e => e.WhatsAppId).HasMaxLength(100);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.OAB).IsUnique();
        // Map PasswordHash column (nullable)
        builder.Property(e => e.PasswordHash).HasMaxLength(1000).IsRequired(false);
    }
}
