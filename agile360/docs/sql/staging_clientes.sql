-- ============================================================
-- Agile360 – Fila de aprovação de cadastros via WhatsApp/n8n
--
-- O bot (n8n) grava aqui. O advogado revisa no painel e confirma.
-- Registros confirmados são movidos para public.clientes.
-- Registros expiram em 24h se não forem revisados.
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================

CREATE TABLE IF NOT EXISTS public.staging_clientes (
    id                  uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
    advogado_id         uuid            NOT NULL,

    -- Dados do cliente (mesma estrutura de clientes)
    tipo_pessoa         varchar(20)     NOT NULL DEFAULT 'PessoaFisica',
    nome                varchar(200),
    cpf                 varchar(14),
    rg                  varchar(20),
    orgao_expedidor     varchar(20),
    razao_social        varchar(200),
    cnpj                varchar(18),
    inscricao_estadual  varchar(20),
    email               varchar(256),
    telefone            varchar(20),
    whatsapp_numero     varchar(20),
    data_referencia     date,
    area_atuacao        varchar(200),
    endereco            varchar(500),
    observacoes         text,
    origem              varchar(20)     NOT NULL DEFAULT 'WhatsApp',
    origem_mensagem     text,           -- texto bruto original enviado no WhatsApp

    -- Controle de staging
    status              varchar(20)     NOT NULL DEFAULT 'Pendente'
                            CHECK (status IN ('Pendente','Confirmado','Rejeitado')),
    expires_at          timestamptz     NOT NULL DEFAULT (now() + interval '24 hours'),
    confirmado_em       timestamptz,    -- preenchido ao confirmar
    rejeitado_em        timestamptz,    -- preenchido ao rejeitar
    cliente_id_gerado   uuid,           -- id do cliente criado em clientes após confirmação

    created_at          timestamptz     NOT NULL DEFAULT now(),
    updated_at          timestamptz     NOT NULL DEFAULT now()
);

-- Lookup rápido para o card do dashboard: "quantos pendentes tem esse advogado?"
CREATE INDEX IF NOT EXISTS idx_staging_advogado_status
    ON public.staging_clientes (advogado_id, status)
    WHERE status = 'Pendente';

-- Limpeza periódica de expirados
CREATE INDEX IF NOT EXISTS idx_staging_expires
    ON public.staging_clientes (expires_at)
    WHERE status = 'Pendente';

-- ── RLS ──────────────────────────────────────────────────────
ALTER TABLE public.staging_clientes ENABLE ROW LEVEL SECURITY;

CREATE POLICY "service_role_only" ON public.staging_clientes
    AS RESTRICTIVE FOR ALL TO service_role
    USING (true) WITH CHECK (true);

REVOKE ALL ON public.staging_clientes FROM anon, authenticated;

COMMENT ON TABLE  public.staging_clientes IS 'Fila de aprovação: registros enviados pelo bot aguardam revisão do advogado antes de entrar em clientes.';
COMMENT ON COLUMN public.staging_clientes.origem_mensagem IS 'Texto bruto original da mensagem do WhatsApp — para o advogado ter contexto ao revisar.';
COMMENT ON COLUMN public.staging_clientes.expires_at IS 'Registro expira em 24h se não for revisado. Um worker de limpeza pode deletar os expirados.';
