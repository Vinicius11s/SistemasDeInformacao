using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agile360.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditShadowPropertiesAndAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "processos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "processos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "prazos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "prazos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "notas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "notas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "audiencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "audiencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_AdvogadoId",
                table: "audit_logs",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ChangedAt",
                table: "audit_logs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "audiencias");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "audiencias");
        }
    }
}
