using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agile360.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaColumnsToAdvogado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Colunas MFA ───────────────────────────────────────────────────────────
            // Adicionadas à tabela "advogado" (Supabase usa o nome singular).
            // O segredo é armazenado criptografado (AES-256-GCM) — nunca em plaintext.

            migrationBuilder.AddColumn<bool>(
                name: "mfa_enabled",
                table: "advogado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "mfa_secret",
                table: "advogado",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mfa_pending_secret",
                table: "advogado",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // ── foto_url — estava no InitialSchema (advogados) mas faltou na tabela advogado ──
            // Coluna de perfil existente no Domain; necessária para o fluxo de MFA (GetAdvogadoAsync).

            migrationBuilder.AddColumn<string>(
                name: "foto_url",
                table: "advogado",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // ── Colunas de perfil / assinatura (adicionadas à entidade após InitialSchema) ─

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "advogado",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_escritorio",
                table: "advogado",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf_cnpj",
                table: "advogado",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "advogado",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "advogado",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "plano",
                table: "advogado",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_assinatura",
                table: "advogado",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // data_expiracao é tipo "date" no Postgres (sem timezone).
            // DateOnly no C# ↔ "date" no Postgres via Npgsql — sem cast.
            migrationBuilder.AddColumn<DateOnly>(
                name: "data_expiracao",
                table: "advogado",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_customer_id",
                table: "advogado",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "advogado",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            // is_active substitui a coluna "Ativo" da InitialSchema (renomeada na entidade)
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "advogado",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "mfa_enabled",         table: "advogado");
            migrationBuilder.DropColumn(name: "mfa_secret",          table: "advogado");
            migrationBuilder.DropColumn(name: "mfa_pending_secret",  table: "advogado");
            migrationBuilder.DropColumn(name: "foto_url",            table: "advogado");
            migrationBuilder.DropColumn(name: "role",                table: "advogado");
            migrationBuilder.DropColumn(name: "nome_escritorio",     table: "advogado");
            migrationBuilder.DropColumn(name: "cpf_cnpj",            table: "advogado");
            migrationBuilder.DropColumn(name: "cidade",              table: "advogado");
            migrationBuilder.DropColumn(name: "estado",              table: "advogado");
            migrationBuilder.DropColumn(name: "plano",               table: "advogado");
            migrationBuilder.DropColumn(name: "status_assinatura",   table: "advogado");
            migrationBuilder.DropColumn(name: "data_expiracao",      table: "advogado");
            migrationBuilder.DropColumn(name: "stripe_customer_id",  table: "advogado");
            migrationBuilder.DropColumn(name: "password_hash",       table: "advogado");
            migrationBuilder.DropColumn(name: "is_active",           table: "advogado");
        }
    }
}
