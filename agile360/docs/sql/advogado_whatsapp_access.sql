-- ============================================================
-- Hub Central (n8n) — Accesso de leitura para `advogado_whatsapp`
-- ============================================================
-- Objetivo: n8n fazer lookup performático por whatsapp_numero
-- para obter id_advogado (UUID) e então chamar a staging.
--
-- Se a tabela já existe (como descrito), este script:
-- - garante índice em whatsapp_numero
-- - adiciona policy de SELECT para service_role (Supabase)
-- ============================================================

-- 1) Índice para lookup por número
CREATE INDEX IF NOT EXISTS idx_advogado_whatsapp_whatsapp_numero
ON public.advogado_whatsapp (whatsapp_numero);

-- 2) RLS (se estiver habilitado no seu Supabase)
ALTER TABLE public.advogado_whatsapp ENABLE ROW LEVEL SECURITY;

-- 3) Permitir leitura para service_role (n8n usa credencial admin/service_role)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename  = 'advogado_whatsapp'
          AND policyname = 'service_role_only'
    ) THEN
        CREATE POLICY "service_role_only"
            ON public.advogado_whatsapp
            AS RESTRICTIVE
            FOR SELECT
            TO service_role
            USING (true);
    END IF;
END $$;

