using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agile360.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advogados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OAB = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WhatsAppId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advogados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audiencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Local = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    GoogleEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LembretesEnviados = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audiencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CPF = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    RG = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WhatsAppNumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entradas_ia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConteudoOriginal = table.Column<string>(type: "text", nullable: false),
                    DadosExtraidos = table.Column<string>(type: "text", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas_ia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: false),
                    Fixada = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prazos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataVencimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Prioridade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlertaEnviado = table.Column<bool>(type: "boolean", nullable: false),
                    OrigemIntimacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prazos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroProcesso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Vara = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comarca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tribunal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TipoAcao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ValorCausa = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    DataDistribuicao = table.Column<DateOnly>(type: "date", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advogados_Email",
                table: "advogados",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_advogados_OAB",
                table: "advogados",
                column: "OAB",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audiencias_AdvogadoId",
                table: "audiencias",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_audiencias_AdvogadoId_DataHora_Status",
                table: "audiencias",
                columns: new[] { "AdvogadoId", "DataHora", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_audiencias_DataHora",
                table: "audiencias",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_AdvogadoId",
                table: "clientes",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_AdvogadoId_CPF",
                table: "clientes",
                columns: new[] { "AdvogadoId", "CPF" });

            migrationBuilder.CreateIndex(
                name: "IX_entradas_ia_AdvogadoId",
                table: "entradas_ia",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_notas_AdvogadoId",
                table: "notas",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_prazos_AdvogadoId",
                table: "prazos",
                column: "AdvogadoId");

            migrationBuilder.CreateIndex(
                name: "IX_prazos_AdvogadoId_DataVencimento_Status",
                table: "prazos",
                columns: new[] { "AdvogadoId", "DataVencimento", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_prazos_DataVencimento",
                table: "prazos",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "IX_processos_AdvogadoId_NumeroProcesso",
                table: "processos",
                columns: new[] { "AdvogadoId", "NumeroProcesso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processos_AdvogadoId_Status",
                table: "processos",
                columns: new[] { "AdvogadoId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advogados");

            migrationBuilder.DropTable(
                name: "audiencias");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "entradas_ia");

            migrationBuilder.DropTable(
                name: "notas");

            migrationBuilder.DropTable(
                name: "prazos");

            migrationBuilder.DropTable(
                name: "processos");
        }
    }
}
