using Agile360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agile360.Infrastructure.Data.Configurations;

/// <summary>
/// Mapeamento Fluent API para a entidade <see cref="RecoveryCode"/>.
///
/// Tabela: public.advogado_recovery_codes
///
/// Políticas de segurança no mapeamento:
///   - code_hash: varchar(100) — BCrypt hash; nunca o plaintext.
///   - Índice parcial em (advogado_id, is_used) para busca rápida dos códigos ativos.
///   - FK com CASCADE DELETE → ao excluir o advogado, todos os códigos são removidos.
///   - CreatedAt / UsedAt → timestamptz para auditoria completa com fuso horário.
/// </summary>
public class RecoveryCodeConfiguration : IEntityTypeConfiguration<RecoveryCode>
{
    public void Configure(EntityTypeBuilder<RecoveryCode> builder)
    {
        builder.ToTable("advogado_recovery_codes");
        builder.HasKey(e => e.Id);

        // ─── CodeHash — armazena apenas o BCrypt hash, nunca plaintext ───────────
        builder.Property(e => e.CodeHash)
               .HasColumnName("code_hash")
               .HasMaxLength(100)
               .IsRequired();

        // ─── Flags de uso ─────────────────────────────────────────────────────────
        builder.Property(e => e.IsUsed)
               .HasColumnName("is_used")
               .HasDefaultValue(false)
               .IsRequired();

        // ─── Timestamps de auditoria ──────────────────────────────────────────────
        builder.Property(e => e.UsedAt)
               .HasColumnName("used_at")
               .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamp with time zone")
               .IsRequired();

        // ─── FK com cascade delete ────────────────────────────────────────────────
        // Ao excluir o advogado, todos os recovery codes são removidos automaticamente.
        builder.Property(e => e.AdvogadoId)
               .HasColumnName("advogado_id")
               .IsRequired();

        builder.HasOne(e => e.Advogado)
               .WithMany()
               .HasForeignKey(e => e.AdvogadoId)
               .OnDelete(DeleteBehavior.Cascade);

        // ─── Índice composto ──────────────────────────────────────────────────────
        // Otimiza a busca "códigos ativos de um advogado" — operação crítica no login.
        builder.HasIndex(e => new { e.AdvogadoId, e.IsUsed })
               .HasDatabaseName("ix_recovery_codes_advogado_active");
    }
}
