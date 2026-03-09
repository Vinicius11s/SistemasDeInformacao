-- ============================================================
-- Agile360 – API Keys para integrações M2M (n8n, WhatsApp, etc.)
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================

CREATE TABLE IF NOT EXISTS public.api_keys (
    id              uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    advogado_id     uuid        NOT NULL,
    name            varchar(100) NOT NULL,           -- nome amigável: "n8n Produção"
    key_hash        varchar(64)  NOT NULL UNIQUE,    -- SHA-256 hex da chave raw
    key_prefix      varchar(12)  NOT NULL,           -- primeiros 8 chars (ex: "a360_live") para identificação sem revelar a chave
    created_at      timestamptz  NOT NULL DEFAULT now(),
    last_used_at    timestamptz,
    expires_at      timestamptz,                     -- NULL = não expira
    revoked_at      timestamptz                      -- NULL = ativa
);

-- Lookup eficiente por hash (caminho crítico de toda requisição autenticada)
CREATE UNIQUE INDEX IF NOT EXISTS idx_api_keys_hash
    ON public.api_keys (key_hash)
    WHERE revoked_at IS NULL;

-- Listar chaves de um advogado
CREATE INDEX IF NOT EXISTS idx_api_keys_advogado
    ON public.api_keys (advogado_id);

-- ── RLS ──────────────────────────────────────────────────────
ALTER TABLE public.api_keys ENABLE ROW LEVEL SECURITY;

CREATE POLICY "service_role_only" ON public.api_keys
    AS RESTRICTIVE FOR ALL TO service_role
    USING (true) WITH CHECK (true);

REVOKE ALL ON public.api_keys FROM anon, authenticated;

COMMENT ON TABLE  public.api_keys IS 'Chaves de API para integração M2M (n8n, webhooks, automações).';
COMMENT ON COLUMN public.api_keys.key_hash   IS 'SHA-256 hex da chave raw. A chave raw é mostrada UMA VEZ no momento da criação.';
COMMENT ON COLUMN public.api_keys.key_prefix IS 'Prefixo visível (ex: a360_AbCd) para o advogado identificar qual chave é qual.';
