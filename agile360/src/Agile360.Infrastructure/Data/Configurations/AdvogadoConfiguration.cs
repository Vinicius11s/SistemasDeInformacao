using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class AdvogadoConfiguration : IEntityTypeConfiguration<Advogado>
{
    public void Configure(EntityTypeBuilder<Advogado> builder)
    {
        builder.ToTable("advogado");
        builder.HasKey(e => e.Id);

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.Email)        .IsRequired().HasMaxLength(256);
        builder.Property(e => e.Nome)         .IsRequired().HasMaxLength(200);
        builder.Property(e => e.OAB)          .HasMaxLength(20);
        builder.Property(e => e.Telefone)     .HasMaxLength(20);
        builder.Property(e => e.FotoUrl)      .HasMaxLength(500);
        builder.Property(e => e.PasswordHash) .HasMaxLength(1000).IsRequired(false);

        // WhatsAppId → convenção gera "whats_app_id"; banco provavelmente tem "whatsapp_id"
        builder.Property(e => e.WhatsAppId).HasColumnName("whatsapp_id").HasMaxLength(100);

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.OAB).IsUnique();
    }
}
