using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_key");
        builder.HasKey(e => e.Id);

        // FK: banco usa id_ prefix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Schema real (Postgres): snake_case
        // Esses mapeamentos explícitos evitam "column does not exist" (42703)
        // caso a convenção global não esteja aplicada em algum cenário.
        builder.Property(e => e.KeyHash)  .HasColumnName("key_hash");
        builder.Property(e => e.KeyPrefix).HasColumnName("key_prefix");
        builder.Property(e => e.Ativa)    .HasColumnName("ativa");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");

        // Propriedade C# "Name" mapeia para coluna "nome_dispositivo" (nome não-convencional)
        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("nome_dispositivo");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.KeyHash)  .IsRequired().HasMaxLength(64);
        builder.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(12);
        builder.Property(e => e.Ativa)    .IsRequired();

        builder.HasIndex(e => e.KeyHash).IsUnique();
        builder.HasIndex(e => e.AdvogadoId);

        // IsActive é calculado — sem coluna no banco
        builder.Ignore(e => e.IsActive);
    }
}
