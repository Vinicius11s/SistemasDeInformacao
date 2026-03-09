-- ============================================================
-- Agile360 – Refresh Token Sessions (Security Hardening)
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================

CREATE TABLE IF NOT EXISTS public.refresh_token_sessions (
    id              uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
    advogado_id     uuid            NOT NULL,
    token_hash      text            NOT NULL,   -- SHA-256 hex do refresh token raw
    expires_at      timestamptz     NOT NULL,
    created_at      timestamptz     NOT NULL DEFAULT now(),
    revoked_at      timestamptz,               -- NULL = ativo; NOT NULL = revogado
    user_agent      text,
    ip_address      text
);

-- Índice para lookup rápido por hash (fluxo /refresh)
CREATE UNIQUE INDEX IF NOT EXISTS idx_rts_token_hash
    ON public.refresh_token_sessions (token_hash)
    WHERE revoked_at IS NULL;

-- Índice para revogar todas as sessões de um advogado (/logout all)
CREATE INDEX IF NOT EXISTS idx_rts_advogado_id
    ON public.refresh_token_sessions (advogado_id);

-- Índice para limpeza de sessões expiradas (job periódico)
CREATE INDEX IF NOT EXISTS idx_rts_expires_at
    ON public.refresh_token_sessions (expires_at)
    WHERE revoked_at IS NULL;

-- ── RLS ──────────────────────────────────────────────────────
ALTER TABLE public.refresh_token_sessions ENABLE ROW LEVEL SECURITY;

-- Apenas o service_role (backend) pode ler/gravar; nenhum usuário anon acessa
CREATE POLICY "service_role_only" ON public.refresh_token_sessions
    AS RESTRICTIVE
    FOR ALL
    TO service_role
    USING (true)
    WITH CHECK (true);

-- Negar acesso anon explicitamente
REVOKE ALL ON public.refresh_token_sessions FROM anon, authenticated;

-- ── Helper: limpar sessões expiradas (chamar via cron ou pg_cron) ──
-- SELECT cron.schedule('cleanup-refresh-tokens','0 3 * * *',
--   $$DELETE FROM public.refresh_token_sessions WHERE expires_at < now()$$);
