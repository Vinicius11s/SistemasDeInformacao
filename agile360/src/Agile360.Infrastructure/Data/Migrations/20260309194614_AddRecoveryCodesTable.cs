using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agile360.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoveryCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audiencias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_processos",
                table: "processos");

            migrationBuilder.DropIndex(
                name: "IX_processos_AdvogadoId_NumeroProcesso",
                table: "processos");

            migrationBuilder.DropIndex(
                name: "IX_processos_AdvogadoId_Status",
                table: "processos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prazos",
                table: "prazos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notas",
                table: "notas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_entradas_ia",
                table: "entradas_ia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_clientes",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "IX_clientes_AdvogadoId_CPF",
                table: "clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "Comarca",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "NumeroProcesso",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "TipoAcao",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "UltimaMovimentacao",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "Vara",
                table: "processos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "OrigemIntimacao",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "prazos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "notas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "entradas_ia");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "entradas_ia");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "clientes");

            migrationBuilder.RenameTable(
                name: "processos",
                newName: "processo");

            migrationBuilder.RenameTable(
                name: "prazos",
                newName: "prazo");

            migrationBuilder.RenameTable(
                name: "notas",
                newName: "nota");

            migrationBuilder.RenameTable(
                name: "entradas_ia",
                newName: "entrada_ia");

            migrationBuilder.RenameTable(
                name: "clientes",
                newName: "cliente");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                newName: "audit_log");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "advogado",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "plano",
                table: "advogado",
                newName: "Plano");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "advogado",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "cidade",
                table: "advogado",
                newName: "Cidade");

            migrationBuilder.RenameColumn(
                name: "stripe_customer_id",
                table: "advogado",
                newName: "StripeCustomerId");

            migrationBuilder.RenameColumn(
                name: "status_assinatura",
                table: "advogado",
                newName: "StatusAssinatura");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "advogado",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "nome_escritorio",
                table: "advogado",
                newName: "NomeEscritorio");

            migrationBuilder.RenameColumn(
                name: "mfa_secret",
                table: "advogado",
                newName: "MfaSecret");

            migrationBuilder.RenameColumn(
                name: "mfa_pending_secret",
                table: "advogado",
                newName: "MfaPendingSecret");

            migrationBuilder.RenameColumn(
                name: "mfa_enabled",
                table: "advogado",
                newName: "MfaEnabled");

            migrationBuilder.RenameColumn(
                name: "foto_url",
                table: "advogado",
                newName: "FotoUrl");

            migrationBuilder.RenameColumn(
                name: "cpf_cnpj",
                table: "advogado",
                newName: "CpfCnpj");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "processo",
                newName: "id_cliente");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "processo",
                newName: "id_advogado");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "processo",
                newName: "ParteContraria");

            migrationBuilder.RenameColumn(
                name: "ProcessoId",
                table: "prazo",
                newName: "id_processo");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "prazo",
                newName: "id_advogado");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "prazo",
                newName: "TipoContagem");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "prazo",
                newName: "id_cliente");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "prazo",
                newName: "SuspensaoPrazos");

            migrationBuilder.RenameColumn(
                name: "AlertaEnviado",
                table: "prazo",
                newName: "LembreteEnviado");

            migrationBuilder.RenameIndex(
                name: "IX_prazos_DataVencimento",
                table: "prazo",
                newName: "IX_prazo_DataVencimento");

            migrationBuilder.RenameIndex(
                name: "IX_prazos_AdvogadoId_DataVencimento_Status",
                table: "prazo",
                newName: "IX_prazo_id_advogado_DataVencimento_Status");

            migrationBuilder.RenameIndex(
                name: "IX_prazos_AdvogadoId",
                table: "prazo",
                newName: "IX_prazo_id_advogado");

            migrationBuilder.RenameColumn(
                name: "ProcessoId",
                table: "nota",
                newName: "id_processo");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "nota",
                newName: "id_advogado");

            migrationBuilder.RenameIndex(
                name: "IX_notas_AdvogadoId",
                table: "nota",
                newName: "IX_nota_id_advogado");

            migrationBuilder.RenameColumn(
                name: "ProcessoId",
                table: "entrada_ia",
                newName: "id_processo");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "entrada_ia",
                newName: "id_cliente");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "entrada_ia",
                newName: "id_advogado");

            migrationBuilder.RenameIndex(
                name: "IX_entradas_ia_AdvogadoId",
                table: "entrada_ia",
                newName: "IX_entrada_ia_id_advogado");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "cliente",
                newName: "id_advogado");

            migrationBuilder.RenameColumn(
                name: "WhatsAppNumero",
                table: "cliente",
                newName: "OrgaoExpedidor");

            migrationBuilder.RenameColumn(
                name: "Origem",
                table: "cliente",
                newName: "TipoCliente");

            migrationBuilder.RenameIndex(
                name: "IX_clientes_AdvogadoId",
                table: "cliente",
                newName: "IX_cliente_id_advogado");

            migrationBuilder.RenameColumn(
                name: "AdvogadoId",
                table: "audit_log",
                newName: "id_advogado");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                table: "audit_log",
                newName: "IX_audit_log_EntityName_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_ChangedAt",
                table: "audit_log",
                newName: "IX_audit_log_ChangedAt");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_AdvogadoId",
                table: "audit_log",
                newName: "IX_audit_log_id_advogado");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_cliente",
                table: "processo",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Assunto",
                table: "processo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComarcaVara",
                table: "processo",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CriadoEm",
                table: "processo",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "FaseProcessual",
                table: "processo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HonorariosEstimados",
                table: "processo",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumProcesso",
                table: "processo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "processo",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "prazo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataVencimento",
                table: "prazo",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_processo",
                table: "prazo",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CriadoEm",
                table: "prazo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DataConclusao",
                table: "prazo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataPublicacao",
                table: "prazo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrazoDias",
                table: "prazo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPrazo",
                table: "prazo",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "prazo",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "cliente",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaAtuacao",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CEP",
                table: "cliente",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CNPJ",
                table: "cliente",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataCadastro",
                table: "cliente",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataReferencia",
                table: "cliente",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InscricaoEstadual",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeCompleto",
                table: "cliente",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroConta",
                table: "cliente",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pix",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "cliente",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_processo",
                table: "processo",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prazo",
                table: "prazo",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nota",
                table: "nota",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_entrada_ia",
                table: "entrada_ia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cliente",
                table: "cliente",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "advogado_recovery_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    advogado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advogado_recovery_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_advogado_recovery_codes_advogado_advogado_id",
                        column: x => x.advogado_id,
                        principalTable: "advogado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_key",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_advogado = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_dispositivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_key", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compromisso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoCompromisso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TipoAudiencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Local = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: true),
                    id_processo = table.Column<Guid>(type: "uuid", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    LembreteMinutos = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateOnly>(type: "date", nullable: false),
                    id_advogado = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compromisso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token_session",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_advogado = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token_session", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staging_cliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_advogado = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoPessoa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CPF = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    RG = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OrgaoExpedidor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RazaoSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CNPJ = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WhatsAppNumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataReferencia = table.Column<DateOnly>(type: "date", nullable: true),
                    AreaAtuacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrigemMensagem = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejeitadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClienteIdGerado = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staging_cliente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processo_id_advogado",
                table: "processo",
                column: "id_advogado");

            migrationBuilder.CreateIndex(
                name: "IX_processo_id_advogado_NumProcesso",
                table: "processo",
                columns: new[] { "id_advogado", "NumProcesso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_CNPJ",
                table: "cliente",
                column: "CNPJ");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_CPF",
                table: "cliente",
                column: "CPF");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_codes_advogado_active",
                table: "advogado_recovery_codes",
                columns: new[] { "advogado_id", "is_used" });

            migrationBuilder.CreateIndex(
                name: "IX_api_key_id_advogado",
                table: "api_key",
                column: "id_advogado");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_KeyHash",
                table: "api_key",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compromisso_id_advogado",
                table: "compromisso",
                column: "id_advogado");

            migrationBuilder.CreateIndex(
                name: "IX_compromisso_id_advogado_Data",
                table: "compromisso",
                columns: new[] { "id_advogado", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_session_ExpiresAt",
                table: "refresh_token_session",
                column: "ExpiresAt",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_session_id_advogado",
                table: "refresh_token_session",
                column: "id_advogado");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_session_TokenHash",
                table: "refresh_token_session",
                column: "TokenHash",
                unique: true,
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staging_cliente_ExpiresAt",
                table: "staging_cliente",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_staging_cliente_id_advogado_Status",
                table: "staging_cliente",
                columns: new[] { "id_advogado", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advogado_recovery_codes");

            migrationBuilder.DropTable(
                name: "api_key");

            migrationBuilder.DropTable(
                name: "compromisso");

            migrationBuilder.DropTable(
                name: "refresh_token_session");

            migrationBuilder.DropTable(
                name: "staging_cliente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_processo",
                table: "processo");

            migrationBuilder.DropIndex(
                name: "IX_processo_id_advogado",
                table: "processo");

            migrationBuilder.DropIndex(
                name: "IX_processo_id_advogado_NumProcesso",
                table: "processo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prazo",
                table: "prazo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nota",
                table: "nota");

            migrationBuilder.DropPrimaryKey(
                name: "PK_entrada_ia",
                table: "entrada_ia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cliente",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "IX_cliente_CNPJ",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "IX_cliente_CPF",
                table: "cliente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "Assunto",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "ComarcaVara",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "FaseProcessual",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "HonorariosEstimados",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "NumProcesso",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "processo");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "DataPublicacao",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "PrazoDias",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "TipoPrazo",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "prazo");

            migrationBuilder.DropColumn(
                name: "AreaAtuacao",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "CEP",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "CNPJ",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "DataReferencia",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "InscricaoEstadual",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "NomeCompleto",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "NumeroConta",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "Pix",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "cliente");

            migrationBuilder.RenameTable(
                name: "processo",
                newName: "processos");

            migrationBuilder.RenameTable(
                name: "prazo",
                newName: "prazos");

            migrationBuilder.RenameTable(
                name: "nota",
                newName: "notas");

            migrationBuilder.RenameTable(
                name: "entrada_ia",
                newName: "entradas_ia");

            migrationBuilder.RenameTable(
                name: "cliente",
                newName: "clientes");

            migrationBuilder.RenameTable(
                name: "audit_log",
                newName: "audit_logs");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "advogado",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Plano",
                table: "advogado",
                newName: "plano");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "advogado",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "Cidade",
                table: "advogado",
                newName: "cidade");

            migrationBuilder.RenameColumn(
                name: "StripeCustomerId",
                table: "advogado",
                newName: "stripe_customer_id");

            migrationBuilder.RenameColumn(
                name: "StatusAssinatura",
                table: "advogado",
                newName: "status_assinatura");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "advogado",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NomeEscritorio",
                table: "advogado",
                newName: "nome_escritorio");

            migrationBuilder.RenameColumn(
                name: "MfaSecret",
                table: "advogado",
                newName: "mfa_secret");

            migrationBuilder.RenameColumn(
                name: "MfaPendingSecret",
                table: "advogado",
                newName: "mfa_pending_secret");

            migrationBuilder.RenameColumn(
                name: "MfaEnabled",
                table: "advogado",
                newName: "mfa_enabled");

            migrationBuilder.RenameColumn(
                name: "FotoUrl",
                table: "advogado",
                newName: "foto_url");

            migrationBuilder.RenameColumn(
                name: "CpfCnpj",
                table: "advogado",
                newName: "cpf_cnpj");

            migrationBuilder.RenameColumn(
                name: "id_cliente",
                table: "processos",
                newName: "ClienteId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "processos",
                newName: "AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "ParteContraria",
                table: "processos",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "id_processo",
                table: "prazos",
                newName: "ProcessoId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "prazos",
                newName: "AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "id_cliente",
                table: "prazos",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "TipoContagem",
                table: "prazos",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SuspensaoPrazos",
                table: "prazos",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "LembreteEnviado",
                table: "prazos",
                newName: "AlertaEnviado");

            migrationBuilder.RenameIndex(
                name: "IX_prazo_id_advogado_DataVencimento_Status",
                table: "prazos",
                newName: "IX_prazos_AdvogadoId_DataVencimento_Status");

            migrationBuilder.RenameIndex(
                name: "IX_prazo_id_advogado",
                table: "prazos",
                newName: "IX_prazos_AdvogadoId");

            migrationBuilder.RenameIndex(
                name: "IX_prazo_DataVencimento",
                table: "prazos",
                newName: "IX_prazos_DataVencimento");

            migrationBuilder.RenameColumn(
                name: "id_processo",
                table: "notas",
                newName: "ProcessoId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "notas",
                newName: "AdvogadoId");

            migrationBuilder.RenameIndex(
                name: "IX_nota_id_advogado",
                table: "notas",
                newName: "IX_notas_AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "id_processo",
                table: "entradas_ia",
                newName: "ProcessoId");

            migrationBuilder.RenameColumn(
                name: "id_cliente",
                table: "entradas_ia",
                newName: "ClienteId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "entradas_ia",
                newName: "AdvogadoId");

            migrationBuilder.RenameIndex(
                name: "IX_entrada_ia_id_advogado",
                table: "entradas_ia",
                newName: "IX_entradas_ia_AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "clientes",
                newName: "AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "TipoCliente",
                table: "clientes",
                newName: "Origem");

            migrationBuilder.RenameColumn(
                name: "OrgaoExpedidor",
                table: "clientes",
                newName: "WhatsAppNumero");

            migrationBuilder.RenameIndex(
                name: "IX_cliente_id_advogado",
                table: "clientes",
                newName: "IX_clientes_AdvogadoId");

            migrationBuilder.RenameColumn(
                name: "id_advogado",
                table: "audit_logs",
                newName: "AdvogadoId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_log_id_advogado",
                table: "audit_logs",
                newName: "IX_audit_logs_AdvogadoId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_log_EntityName_EntityId",
                table: "audit_logs",
                newName: "IX_audit_logs_EntityName_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_log_ChangedAt",
                table: "audit_logs",
                newName: "IX_audit_logs_ChangedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "processos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comarca",
                table: "processos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "processos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "processos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "processos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "processos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroProcesso",
                table: "processos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoAcao",
                table: "processos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaMovimentacao",
                table: "processos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "processos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Vara",
                table: "processos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "prazos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DataVencimento",
                table: "prazos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcessoId",
                table: "prazos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "prazos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "prazos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrigemIntimacao",
                table: "prazos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "prazos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "notas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "notas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "entradas_ia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "entradas_ia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "clientes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_processos",
                table: "processos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prazos",
                table: "prazos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notas",
                table: "notas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_entradas_ia",
                table: "entradas_ia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_clientes",
                table: "clientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "audiencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvogadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DataHora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GoogleEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LembretesEnviados = table.Column<int>(type: "integer", nullable: false),
                    Local = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    ProcessoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audiencias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processos_AdvogadoId_NumeroProcesso",
                table: "processos",
                columns: new[] { "AdvogadoId", "NumeroProcesso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processos_AdvogadoId_Status",
                table: "processos",
                columns: new[] { "AdvogadoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_AdvogadoId_CPF",
                table: "clientes",
                columns: new[] { "AdvogadoId", "CPF" });

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
        }
    }
}
