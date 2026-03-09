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
