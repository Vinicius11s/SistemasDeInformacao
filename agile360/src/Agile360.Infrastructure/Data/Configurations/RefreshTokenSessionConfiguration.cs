using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class RefreshTokenSessionConfiguration : IEntityTypeConfiguration<RefreshTokenSession>
{
    public void Configure(EntityTypeBuilder<RefreshTokenSession> builder)
    {
        builder.ToTable("refresh_token_session");
        builder.HasKey(e => e.Id);

        // FK: banco usa id_ prefix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.IpAddress).HasMaxLength(45);

        // IsActive é propriedade calculada — sem coluna
        builder.Ignore(e => e.IsActive);

        // HasFilter usa snake_case (alinhado com UseSnakeCaseNamingConvention)
        builder.HasIndex(e => e.TokenHash).IsUnique()
            .HasFilter("revoked_at IS NULL");

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => e.ExpiresAt)
            .HasFilter("revoked_at IS NULL");
    }
}
