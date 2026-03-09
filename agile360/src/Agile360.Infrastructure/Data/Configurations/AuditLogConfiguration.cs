using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(e => e.Id);

        // FK: banco usa id_ prefix
        builder.Property(e => e.AdvogadoId).HasColumnName("id_advogado");

        // Constraints — nomes gerados automaticamente pelo UseSnakeCaseNamingConvention
        builder.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Action)    .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.IpAddress) .HasMaxLength(45);

        builder.HasIndex(e => e.AdvogadoId);
        builder.HasIndex(e => new { e.EntityName, e.EntityId });
        builder.HasIndex(e => e.ChangedAt);
    }
}
