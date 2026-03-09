-- ============================================================
-- Agile360 – Coluna n8n_api_key na tabela advogado
--
-- Armazena a chave raw para uso exclusivo do n8n.
-- O admin gera a chave via POST /api/api-keys e cola aqui.
-- O advogado nunca vê ou digita essa chave.
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================

ALTER TABLE public.advogado
    ADD COLUMN IF NOT EXISTS n8n_api_key varchar(60);

-- Nunca expor essa coluna para usuários comuns
-- A RLS já está configurada na tabela advogado, mas garantimos
-- que apenas o service_role (n8n) leia esse campo específico.

COMMENT ON COLUMN public.advogado.n8n_api_key IS
    'Chave API raw para uso exclusivo do n8n. Gerada via POST /api/api-keys pelo admin. Nunca exibida ao advogado.';

-- ── Visão segura para o frontend (exclui n8n_api_key) ──────────
-- Opcional: se quiser que as queries do frontend nunca retornem esse campo
CREATE OR REPLACE VIEW public.advogado_publico AS
    SELECT id, nome, email, telefone, oab, ativo, created_at, updated_at
    FROM public.advogado;
