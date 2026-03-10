-- ============================================================
-- Agile360 – Filas de aprovação de compromissos e processos via WhatsApp/n8n
--
-- Padrão idêntico à staging_cliente:
--   • O bot (n8n) grava aqui.
--   • O advogado revisa no painel e confirma.
--   • Registros confirmados são movidos para as tabelas de produção.
--   • Registros expiram em 24h se não forem revisados.
--   • Todas as colunas em snake_case (padrão Supabase / Postgres).
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================


-- ──────────────────────────────────────────────────────────────────────────
-- 1. staging_compromisso
-- ──────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.staging_compromisso (
    id                      uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
    id_advogado             uuid            NOT NULL,

    -- Dados do compromisso (mesma estrutura de compromisso)
    tipo_compromisso        varchar(100),
    tipo_audiencia          varchar(100),
    data                    date,
    hora                    time,
    local                   varchar(500),
    cliente_nome            varchar(200),   -- nome livre — bot pode não ter o Guid
    num_processo            varchar(50),    -- referência textual ao processo
    observacoes             text,
    lembrete_minutos        integer,
    origem_mensagem         text,           -- texto bruto original do WhatsApp

    -- Controle de staging
    status                  varchar(20)     NOT NULL DEFAULT 'Pendente'
                                CHECK (status IN ('Pendente','Confirmado','Rejeitado')),
    expires_at              timestamptz     NOT NULL DEFAULT (now() + interval '24 hours'),
    confirmado_em           timestamptz,
    rejeitado_em            timestamptz,
    compromisso_id_gerado   uuid,           -- id do compromisso criado após confirmação

    created_at              timestamptz     NOT NULL DEFAULT now(),
    updated_at              timestamptz     NOT NULL DEFAULT now()
);

-- Índice para card do dashboard: "quantos compromissos pendentes?"
CREATE INDEX IF NOT EXISTS idx_staging_comp_advogado_status
    ON public.staging_compromisso (id_advogado, status)
    WHERE status = 'Pendente';

-- Índice para limpeza periódica de expirados
CREATE INDEX IF NOT EXISTS idx_staging_comp_expires
    ON public.staging_compromisso (expires_at)
    WHERE status = 'Pendente';

-- ── RLS ──────────────────────────────────────────────────────────────────
ALTER TABLE public.staging_compromisso ENABLE ROW LEVEL SECURITY;

CREATE POLICY "service_role_only" ON public.staging_compromisso
    AS RESTRICTIVE FOR ALL TO service_role
    USING (true) WITH CHECK (true);

REVOKE ALL ON public.staging_compromisso FROM anon, authenticated;

COMMENT ON TABLE  public.staging_compromisso IS 'Fila de aprovação: compromissos enviados pelo bot aguardam revisão do advogado.';
COMMENT ON COLUMN public.staging_compromisso.origem_mensagem IS 'Texto bruto original da mensagem do WhatsApp — contexto para revisão.';
COMMENT ON COLUMN public.staging_compromisso.expires_at IS 'Registro expira em 24h se não for revisado.';


-- ──────────────────────────────────────────────────────────────────────────
-- 2. staging_processo
-- ──────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.staging_processo (
    id                      uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
    id_advogado             uuid            NOT NULL,

    -- Dados do processo (mesma estrutura de processo)
    num_processo            varchar(50),
    parte_contraria         varchar(200),
    tribunal                varchar(200),
    comarca_vara            varchar(200),
    assunto                 varchar(200),
    valor_causa             numeric(18,2),
    honorarios_estimados    numeric(18,2),
    fase_processual         varchar(100),
    status_processo         varchar(50),    -- status do processo jurídico (Ativo, Arquivado...)
    data_distribuicao       date,
    cliente_nome            varchar(200),   -- nome livre — bot pode não ter o Guid
    observacoes             text,
    origem_mensagem         text,           -- texto bruto original do WhatsApp

    -- Controle de staging
    status                  varchar(20)     NOT NULL DEFAULT 'Pendente'
                                CHECK (status IN ('Pendente','Confirmado','Rejeitado')),
    expires_at              timestamptz     NOT NULL DEFAULT (now() + interval '24 hours'),
    confirmado_em           timestamptz,
    rejeitado_em            timestamptz,
    processo_id_gerado      uuid,           -- id do processo criado após confirmação

    created_at              timestamptz     NOT NULL DEFAULT now(),
    updated_at              timestamptz     NOT NULL DEFAULT now()
);

-- Índice para card do dashboard: "quantos processos pendentes?"
CREATE INDEX IF NOT EXISTS idx_staging_proc_advogado_status
    ON public.staging_processo (id_advogado, status)
    WHERE status = 'Pendente';

-- Índice para limpeza periódica de expirados
CREATE INDEX IF NOT EXISTS idx_staging_proc_expires
    ON public.staging_processo (expires_at)
    WHERE status = 'Pendente';

-- ── RLS ──────────────────────────────────────────────────────────────────
ALTER TABLE public.staging_processo ENABLE ROW LEVEL SECURITY;

CREATE POLICY "service_role_only" ON public.staging_processo
    AS RESTRICTIVE FOR ALL TO service_role
    USING (true) WITH CHECK (true);

REVOKE ALL ON public.staging_processo FROM anon, authenticated;

COMMENT ON TABLE  public.staging_processo IS 'Fila de aprovação: processos enviados pelo bot aguardam revisão do advogado.';
COMMENT ON COLUMN public.staging_processo.origem_mensagem IS 'Texto bruto original da mensagem do WhatsApp — contexto para revisão.';
COMMENT ON COLUMN public.staging_processo.expires_at IS 'Registro expira em 24h se não for revisado.';
