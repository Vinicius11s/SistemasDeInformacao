CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE advogados (
        "Id" uuid NOT NULL,
        "Nome" character varying(200) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "OAB" character varying(20) NOT NULL,
        "Telefone" character varying(20),
        "WhatsAppId" character varying(100),
        "FotoUrl" character varying(500),
        "Ativo" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_advogados" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE audiencias (
        "Id" uuid NOT NULL,
        "ProcessoId" uuid NOT NULL,
        "DataHora" timestamp with time zone NOT NULL,
        "Local" character varying(300),
        "Tipo" character varying(20) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "Observacoes" text,
        "GoogleEventId" character varying(100),
        "LembretesEnviados" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_audiencias" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE clientes (
        "Id" uuid NOT NULL,
        "Nome" character varying(200) NOT NULL,
        "CPF" character varying(14),
        "RG" character varying(20),
        "Email" character varying(256),
        "Telefone" character varying(20),
        "WhatsAppNumero" character varying(20),
        "Endereco" character varying(500),
        "Observacoes" text,
        "Origem" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_clientes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE entradas_ia (
        "Id" uuid NOT NULL,
        "Origem" character varying(20) NOT NULL,
        "ConteudoOriginal" text NOT NULL,
        "DadosExtraidos" text,
        "ClienteId" uuid,
        "ProcessoId" uuid,
        "Status" character varying(20) NOT NULL,
        "ProcessadoEm" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_entradas_ia" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE notas (
        "Id" uuid NOT NULL,
        "ProcessoId" uuid,
        "Titulo" character varying(200) NOT NULL,
        "Conteudo" text NOT NULL,
        "Fixada" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_notas" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE prazos (
        "Id" uuid NOT NULL,
        "ProcessoId" uuid NOT NULL,
        "Descricao" character varying(500) NOT NULL,
        "DataVencimento" timestamp with time zone NOT NULL,
        "Tipo" character varying(20) NOT NULL,
        "Prioridade" character varying(20) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "AlertaEnviado" boolean NOT NULL,
        "OrigemIntimacao" character varying(200),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_prazos" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE TABLE processos (
        "Id" uuid NOT NULL,
        "ClienteId" uuid NOT NULL,
        "NumeroProcesso" character varying(30) NOT NULL,
        "Vara" character varying(100),
        "Comarca" character varying(100),
        "Tribunal" character varying(100),
        "TipoAcao" character varying(200),
        "ValorCausa" numeric,
        "Status" character varying(20) NOT NULL,
        "Descricao" text,
        "DataDistribuicao" date,
        "UltimaMovimentacao" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "AdvogadoId" uuid NOT NULL,
        CONSTRAINT "PK_processos" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_advogados_Email" ON advogados ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_advogados_OAB" ON advogados ("OAB");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_audiencias_AdvogadoId" ON audiencias ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_audiencias_AdvogadoId_DataHora_Status" ON audiencias ("AdvogadoId", "DataHora", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_audiencias_DataHora" ON audiencias ("DataHora");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_clientes_AdvogadoId" ON clientes ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_clientes_AdvogadoId_CPF" ON clientes ("AdvogadoId", "CPF");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_entradas_ia_AdvogadoId" ON entradas_ia ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_notas_AdvogadoId" ON notas ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_prazos_AdvogadoId" ON prazos ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_prazos_AdvogadoId_DataVencimento_Status" ON prazos ("AdvogadoId", "DataVencimento", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_prazos_DataVencimento" ON prazos ("DataVencimento");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_processos_AdvogadoId_NumeroProcesso" ON processos ("AdvogadoId", "NumeroProcesso");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    CREATE INDEX "IX_processos_AdvogadoId_Status" ON processos ("AdvogadoId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221190324_InitialSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260221190324_InitialSchema', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE processos ADD "CreatedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE processos ADD "LastModifiedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE prazos ADD "CreatedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE prazos ADD "LastModifiedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE notas ADD "CreatedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE notas ADD "LastModifiedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE clientes ADD "CreatedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE clientes ADD "LastModifiedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE audiencias ADD "CreatedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    ALTER TABLE audiencias ADD "LastModifiedBy" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    CREATE TABLE audit_logs (
        "Id" uuid NOT NULL,
        "EntityName" character varying(100) NOT NULL,
        "EntityId" uuid NOT NULL,
        "Action" character varying(20) NOT NULL,
        "AdvogadoId" uuid,
        "OldValues" text,
        "NewValues" text,
        "ChangedAt" timestamp with time zone NOT NULL,
        "IpAddress" character varying(45),
        CONSTRAINT "PK_audit_logs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    CREATE INDEX "IX_audit_logs_AdvogadoId" ON audit_logs ("AdvogadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    CREATE INDEX "IX_audit_logs_ChangedAt" ON audit_logs ("ChangedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    CREATE INDEX "IX_audit_logs_EntityName_EntityId" ON audit_logs ("EntityName", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221200001_AddAuditShadowPropertiesAndAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260221200001_AddAuditShadowPropertiesAndAuditLog', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    DROP TABLE audiencias;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP CONSTRAINT "PK_processos";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    DROP INDEX "IX_processos_AdvogadoId_NumeroProcesso";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    DROP INDEX "IX_processos_AdvogadoId_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos DROP CONSTRAINT "PK_prazos";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas DROP CONSTRAINT "PK_notas";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entradas_ia DROP CONSTRAINT "PK_entradas_ia";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP CONSTRAINT "PK_clientes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    DROP INDEX "IX_clientes_AdvogadoId_CPF";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE audit_logs DROP CONSTRAINT "PK_audit_logs";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "Comarca";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "CreatedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "IsActive";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "LastModifiedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "NumeroProcesso";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "TipoAcao";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "UltimaMovimentacao";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos DROP COLUMN "Vara";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos DROP COLUMN "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos DROP COLUMN "CreatedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos DROP COLUMN "OrigemIntimacao";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos DROP COLUMN "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas DROP COLUMN "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas DROP COLUMN "CreatedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas DROP COLUMN "LastModifiedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas DROP COLUMN "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entradas_ia DROP COLUMN "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entradas_ia DROP COLUMN "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "CreatedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "Email";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "LastModifiedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "Nome";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "Observacoes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes DROP COLUMN "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processos RENAME TO processo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazos RENAME TO prazo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE notas RENAME TO nota;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entradas_ia RENAME TO entrada_ia;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE clientes RENAME TO cliente;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE audit_logs RENAME TO audit_log;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN role TO "Role";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN plano TO "Plano";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN estado TO "Estado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN cidade TO "Cidade";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN stripe_customer_id TO "StripeCustomerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN status_assinatura TO "StatusAssinatura";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN password_hash TO "PasswordHash";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN nome_escritorio TO "NomeEscritorio";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN mfa_secret TO "MfaSecret";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN mfa_pending_secret TO "MfaPendingSecret";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN mfa_enabled TO "MfaEnabled";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN foto_url TO "FotoUrl";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE advogado RENAME COLUMN cpf_cnpj TO "CpfCnpj";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo RENAME COLUMN "ClienteId" TO id_cliente;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo RENAME COLUMN "Descricao" TO "ParteContraria";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "ProcessoId" TO id_processo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "Tipo" TO "TipoContagem";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "LastModifiedBy" TO id_cliente;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "IsActive" TO "SuspensaoPrazos";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo RENAME COLUMN "AlertaEnviado" TO "LembreteEnviado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_prazos_DataVencimento" RENAME TO "IX_prazo_DataVencimento";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_prazos_AdvogadoId_DataVencimento_Status" RENAME TO "IX_prazo_id_advogado_DataVencimento_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_prazos_AdvogadoId" RENAME TO "IX_prazo_id_advogado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE nota RENAME COLUMN "ProcessoId" TO id_processo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE nota RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_notas_AdvogadoId" RENAME TO "IX_nota_id_advogado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entrada_ia RENAME COLUMN "ProcessoId" TO id_processo;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entrada_ia RENAME COLUMN "ClienteId" TO id_cliente;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entrada_ia RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_entradas_ia_AdvogadoId" RENAME TO "IX_entrada_ia_id_advogado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente RENAME COLUMN "WhatsAppNumero" TO "OrgaoExpedidor";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente RENAME COLUMN "Origem" TO "TipoCliente";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_clientes_AdvogadoId" RENAME TO "IX_cliente_id_advogado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE audit_log RENAME COLUMN "AdvogadoId" TO id_advogado;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_audit_logs_EntityName_EntityId" RENAME TO "IX_audit_log_EntityName_EntityId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_audit_logs_ChangedAt" RENAME TO "IX_audit_log_ChangedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER INDEX "IX_audit_logs_AdvogadoId" RENAME TO "IX_audit_log_id_advogado";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ALTER COLUMN id_cliente DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "Assunto" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "ComarcaVara" character varying(150);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "CriadoEm" date NOT NULL DEFAULT DATE '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "FaseProcessual" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "HonorariosEstimados" numeric;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "NumProcesso" character varying(50) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD "Observacoes" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ALTER COLUMN "Descricao" TYPE text;
    ALTER TABLE prazo ALTER COLUMN "Descricao" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ALTER COLUMN "DataVencimento" TYPE date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ALTER COLUMN id_processo DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "CriadoEm" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "DataConclusao" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "DataPublicacao" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "PrazoDias" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "TipoPrazo" character varying(30) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD "Titulo" character varying(300) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ALTER COLUMN "Endereco" TYPE character varying(300);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "AreaAtuacao" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Bairro" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "CEP" character varying(9);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "CNPJ" character varying(18);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Cidade" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Complemento" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "DataCadastro" date NOT NULL DEFAULT DATE '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "DataReferencia" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Estado" character varying(2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "EstadoCivil" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "InscricaoEstadual" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "NomeCompleto" character varying(300);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Numero" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "NumeroConta" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "Pix" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD "RazaoSocial" character varying(300);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE processo ADD CONSTRAINT "PK_processo" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE prazo ADD CONSTRAINT "PK_prazo" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE nota ADD CONSTRAINT "PK_nota" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE entrada_ia ADD CONSTRAINT "PK_entrada_ia" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE cliente ADD CONSTRAINT "PK_cliente" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    ALTER TABLE audit_log ADD CONSTRAINT "PK_audit_log" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE TABLE advogado_recovery_codes (
        "Id" uuid NOT NULL,
        advogado_id uuid NOT NULL,
        code_hash character varying(100) NOT NULL,
        is_used boolean NOT NULL DEFAULT FALSE,
        used_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_advogado_recovery_codes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_advogado_recovery_codes_advogado_advogado_id" FOREIGN KEY (advogado_id) REFERENCES advogado ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE TABLE api_key (
        "Id" uuid NOT NULL,
        id_advogado uuid NOT NULL,
        nome_dispositivo character varying(100) NOT NULL,
        "KeyHash" character varying(64) NOT NULL,
        "KeyPrefix" character varying(12) NOT NULL,
        "Ativa" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "LastUsedAt" timestamp with time zone,
        "ExpiresAt" timestamp with time zone,
        "RevokedAt" timestamp with time zone,
        CONSTRAINT "PK_api_key" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE TABLE compromisso (
        "Id" uuid NOT NULL,
        "TipoCompromisso" character varying(50) NOT NULL,
        "TipoAudiencia" character varying(50),
        "Data" date NOT NULL,
        "Hora" time without time zone NOT NULL,
        "Local" character varying(300),
        id_cliente uuid,
        id_processo uuid,
        "Observacoes" text,
        "LembreteMinutos" integer,
        "CriadoEm" date NOT NULL,
        id_advogado uuid NOT NULL,
        CONSTRAINT "PK_compromisso" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE TABLE refresh_token_session (
        "Id" uuid NOT NULL,
        id_advogado uuid NOT NULL,
        "TokenHash" character varying(64) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "UserAgent" character varying(500),
        "IpAddress" character varying(45),
        CONSTRAINT "PK_refresh_token_session" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE TABLE staging_cliente (
        "Id" uuid NOT NULL,
        id_advogado uuid NOT NULL,
        "TipoPessoa" character varying(20) NOT NULL,
        "Nome" character varying(200),
        "CPF" character varying(14),
        "RG" character varying(20),
        "OrgaoExpedidor" character varying(20),
        "RazaoSocial" character varying(200),
        "CNPJ" character varying(18),
        "InscricaoEstadual" character varying(20),
        "Email" character varying(256),
        "Telefone" character varying(20),
        "WhatsAppNumero" character varying(20),
        "DataReferencia" date,
        "AreaAtuacao" character varying(200),
        "Endereco" character varying(500),
        "Observacoes" text,
        "Origem" character varying(20) NOT NULL,
        "OrigemMensagem" text,
        "Status" character varying(20) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "ConfirmadoEm" timestamp with time zone,
        "RejeitadoEm" timestamp with time zone,
        "ClienteIdGerado" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_staging_cliente" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_processo_id_advogado" ON processo (id_advogado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE UNIQUE INDEX "IX_processo_id_advogado_NumProcesso" ON processo (id_advogado, "NumProcesso");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_cliente_CNPJ" ON cliente ("CNPJ");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_cliente_CPF" ON cliente ("CPF");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX ix_recovery_codes_advogado_active ON advogado_recovery_codes (advogado_id, is_used);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_api_key_id_advogado" ON api_key (id_advogado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE UNIQUE INDEX "IX_api_key_KeyHash" ON api_key ("KeyHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_compromisso_id_advogado" ON compromisso (id_advogado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_compromisso_id_advogado_Data" ON compromisso (id_advogado, "Data");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_refresh_token_session_ExpiresAt" ON refresh_token_session ("ExpiresAt") WHERE revoked_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_refresh_token_session_id_advogado" ON refresh_token_session (id_advogado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE UNIQUE INDEX "IX_refresh_token_session_TokenHash" ON refresh_token_session ("TokenHash") WHERE revoked_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_staging_cliente_ExpiresAt" ON staging_cliente ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    CREATE INDEX "IX_staging_cliente_id_advogado_Status" ON staging_cliente (id_advogado, "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260309194614_AddRecoveryCodesTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260309194614_AddRecoveryCodesTable', '9.0.1');
    END IF;
END $EF$;
COMMIT;

