-- ============================================================
-- Script: sync_ef_migrations_history.sql
-- Propósito: "Adopção controlada" do banco Supabase pelo EF Core.
--
-- CONTEXTO:
--   O banco foi inicializado manualmente (Supabase Dashboard / SQL Editor),
--   nunca via `dotnet ef database update`. Por isso:
--     ✗  __EFMigrationsHistory não existe
--     ✗  advogado_recovery_codes não existe
--     ✓  Todas as outras tabelas e colunas existem e estão corretas
--
-- O QUE ESTE SCRIPT FAZ:
--   1. Cria __EFMigrationsHistory (tabela de controle do EF Core)
--   2. Registra as 3 migrations já aplicadas manualmente
--   3. Cria a tabela advogado_recovery_codes (migration pendente)
--   4. Registra a 4ª migration como aplicada
--
-- RESULTADO:
--   O banco ficará 100% sincronizado com o estado do código.
--   Futuras migrações via `dotnet ef database update` funcionarão
--   corretamente sem tentar recriar o que já existe.
--
-- SEGURANÇA:
--   - Todos os DDLs usam IF NOT EXISTS (idempotente, seguro re-executar)
--   - Nenhuma tabela, coluna ou dado existente é modificado
--   - Execute em uma única transação para garantir atomicidade
--
-- INSTRUÇÕES:
--   Supabase Dashboard → SQL Editor → cole e execute
-- ============================================================

BEGIN;

-- ────────────────────────────────────────────────────────────
-- PASSO 1: Criar a tabela de controle de migrações do EF Core
-- ────────────────────────────────────────────────────────────
-- Sem ela, `dotnet ef database update` lança:
--   "relation '__EFMigrationsHistory' does not exist"
-- O nome exato com aspas é obrigatório — EF Core usa case-sensitive.

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    character varying(150) NOT NULL,
    "ProductVersion" character varying(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- ────────────────────────────────────────────────────────────
-- PASSO 2: Registrar migrações já aplicadas manualmente
-- ────────────────────────────────────────────────────────────
-- Estas 3 migrations foram aplicadas via SQL direto no Supabase.
-- Registramos aqui para que o EF Core NÃO tente reaplicá-las
-- (o que causaria erros de "relation already exists").

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
    ('20260221190324_InitialSchema',                      '9.0.1'),
    ('20260221200001_AddAuditShadowPropertiesAndAuditLog','9.0.1'),
    ('20260309120000_AddMfaColumnsToAdvogado',            '9.0.1')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- PASSO 3: Criar tabela de códigos de recuperação MFA
-- ────────────────────────────────────────────────────────────
-- Equivalente ao que a migration 20260309194614_AddRecoveryCodesTable faria.
-- Segue exatamente o schema definido em RecoveryCodeConfiguration.cs.

CREATE TABLE IF NOT EXISTS "advogado_recovery_codes" (
    "Id"          uuid                     NOT NULL,
    "advogado_id" uuid                     NOT NULL,
    "code_hash"   character varying(100)   NOT NULL,
    "is_used"     boolean                  NOT NULL DEFAULT false,
    "used_at"     timestamp with time zone          DEFAULT NULL,
    "created_at"  timestamp with time zone NOT NULL,
    CONSTRAINT "PK_advogado_recovery_codes"
        PRIMARY KEY ("Id"),
    CONSTRAINT "FK_advogado_recovery_codes_advogado_advogado_id"
        FOREIGN KEY ("advogado_id")
        REFERENCES "advogado" ("Id")
        ON DELETE CASCADE
);

-- Índice composto para busca rápida de códigos ativos
-- (operação crítica no login com código de recuperação)
CREATE INDEX IF NOT EXISTS "ix_recovery_codes_advogado_active"
    ON "advogado_recovery_codes" ("advogado_id", "is_used");

-- ────────────────────────────────────────────────────────────
-- PASSO 4: Registrar a migration de recovery codes
-- ────────────────────────────────────────────────────────────

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260309194614_AddRecoveryCodesTable', '9.0.1')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;

-- ────────────────────────────────────────────────────────────
-- VERIFICAÇÃO (execute separadamente após o COMMIT acima)
-- ────────────────────────────────────────────────────────────

-- 1. Confirma que as 4 migrations estão registradas:
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";

-- Resultado esperado (4 linhas):
--  20260221190324_InitialSchema
--  20260221200001_AddAuditShadowPropertiesAndAuditLog
--  20260309120000_AddMfaColumnsToAdvogado
--  20260309194614_AddRecoveryCodesTable

-- 2. Confirma que a tabela de recovery codes foi criada:
SELECT
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name   = 'advogado_recovery_codes'
ORDER BY ordinal_position;

-- Resultado esperado (6 colunas):
--  Id          | uuid                        | NO  |
--  advogado_id | uuid                        | NO  |
--  code_hash   | character varying           | NO  |
--  is_used     | boolean                     | NO  | false
--  used_at     | timestamp with time zone    | YES |
--  created_at  | timestamp with time zone    | NO  |

-- 3. Confirma o índice:
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'advogado_recovery_codes';

-- Resultado esperado (2 entradas):
--  PK_advogado_recovery_codes           | ... PRIMARY KEY ...
--  ix_recovery_codes_advogado_active    | ... (advogado_id, is_used)
