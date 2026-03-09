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

        // ─── Identidade ────────────────────────────────────────────────────────
        builder.Property(e => e.Email)        .IsRequired().HasMaxLength(256);
        builder.Property(e => e.Nome)         .IsRequired().HasMaxLength(200);
        builder.Property(e => e.Role)         .HasMaxLength(50);

        // ─── OAB / Profissional ───────────────────────────────────────────────
        // OAB é acrônimo all-caps; explicitamos "oab" para blindar contra variações
        // de versão do EFCore.NamingConventions (evita risco de gerar "o_a_b").
        builder.Property(e => e.OAB).HasColumnName("oab").HasMaxLength(20);

        // ─── Contato ──────────────────────────────────────────────────────────
        builder.Property(e => e.Telefone)     .HasMaxLength(20);

        // WhatsAppId → convenção gera "whats_app_id"; banco tem "whatsapp_id"
        builder.Property(e => e.WhatsAppId).HasColumnName("whatsapp_id").HasMaxLength(100);

        // ─── Perfil / Escritório ──────────────────────────────────────────────
        builder.Property(e => e.FotoUrl)       .HasMaxLength(500);
        builder.Property(e => e.NomeEscritorio).HasMaxLength(200);
        builder.Property(e => e.CpfCnpj)      .HasMaxLength(18);
        builder.Property(e => e.Cidade)        .HasMaxLength(100);
        builder.Property(e => e.Estado)        .HasMaxLength(2);

        // ─── Assinatura ───────────────────────────────────────────────────────
        builder.Property(e => e.Plano)             .HasMaxLength(50);
        builder.Property(e => e.StatusAssinatura)  .HasMaxLength(50);
        builder.Property(e => e.StripeCustomerId)  .HasMaxLength(100);

        // DataExpiracao → coluna "date" no Postgres (sem timezone).
        // HasColumnType("date") instrui o Npgsql a mapear DateOnly ↔ date
        // sem tentar fazer cast para timestamp/timestamptz (InvalidCastException).
        builder.Property(e => e.DataExpiracao)
               .HasColumnType("date")
               .HasColumnName("data_expiracao");

        // ─── Auth local ───────────────────────────────────────────────────────
        builder.Property(e => e.PasswordHash).HasMaxLength(1000).IsRequired(false);

        // ─── MFA (Google Authenticator) ───────────────────────────────────────
        // UseSnakeCaseNamingConvention converte: MfaEnabled→mfa_enabled,
        // MfaSecret→mfa_secret, MfaPendingSecret→mfa_pending_secret
        builder.Property(e => e.MfaEnabled)       .HasDefaultValue(false);
        builder.Property(e => e.MfaSecret)        .HasMaxLength(500).IsRequired(false);
        builder.Property(e => e.MfaPendingSecret) .HasMaxLength(500).IsRequired(false);

        // ─── Auditoria ────────────────────────────────────────────────────────
        // IsActive → "is_active" (snake_case). Nota: migration InitialSchema usou "Ativo"
        // (sem convenção); o campo foi renomeado para IsActive na entidade.
        builder.Property(e => e.IsActive)
               .HasColumnName("is_active")
               .HasDefaultValue(true);

        // CreatedAt / UpdatedAt → timestamptz no Postgres.
        // Mapeamento explícito evita InvalidCastException caso o banco tenha sido
        // criado manualmente com tipo "date" em vez de "timestamp with time zone".
        // DateTimeOffset no C# ↔ "timestamp with time zone" (timestamptz) no Npgsql.
        builder.Property(e => e.CreatedAt)
               .HasColumnType("timestamp with time zone")
               .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
               .HasColumnType("timestamp with time zone")
               .HasColumnName("updated_at");

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.OAB).IsUnique();
    }
}
