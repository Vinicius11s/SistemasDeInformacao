-- ============================================================
-- Agile360 – MFA / Google Authenticator (TOTP) columns
--
-- Execute no SQL Editor do Supabase como postgres/service_role.
-- mfa_secret é salvo CRIPTOGRAFADO (AES-256-GCM) — nunca em texto puro.
-- ============================================================

ALTER TABLE public.advogado
    ADD COLUMN IF NOT EXISTS mfa_enabled        boolean     NOT NULL DEFAULT false,

    -- Segredo TOTP ativo, criptografado com AES-256-GCM + chave mestra do servidor.
    -- Formato: base64(nonce || ciphertext || tag)
    ADD COLUMN IF NOT EXISTS mfa_secret         text,

    -- Segredo temporário gerado durante o setup, antes da verificação.
    -- Descartado após o primeiro código válido ser confirmado.
    ADD COLUMN IF NOT EXISTS mfa_pending_secret text;

COMMENT ON COLUMN public.advogado.mfa_enabled        IS 'true quando o TOTP foi configurado e verificado pelo advogado';
COMMENT ON COLUMN public.advogado.mfa_secret         IS 'Segredo TOTP criptografado (AES-256-GCM). Nunca salvo em texto puro.';
COMMENT ON COLUMN public.advogado.mfa_pending_secret IS 'Segredo gerado durante o fluxo de setup, descartado após a primeira verificação.';
