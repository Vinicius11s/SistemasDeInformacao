-- Story 1.3: Custom JWT claims (advogado_id, advogado_nome) for the API.
-- Supabase Auth can add these claims via an Auth Hook (Edge Function) that calls this function
-- or by using the "Customize JWT" hook and querying this result.
-- Run in Supabase SQL Editor.

-- Returns claims to be added to the JWT (advogado_id as text, advogado_nome).
-- The Supabase JWT customizer (Dashboard > Authentication > Hooks > Customize JWT) should
-- call this or replicate its logic so the access token contains:
--   "advogado_id": "<uuid>",
--   "advogado_nome": "<string>"
CREATE OR REPLACE FUNCTION public.get_advogado_claims(p_user_id uuid)
RETURNS jsonb
LANGUAGE sql
STABLE
SECURITY DEFINER
SET search_path = public
AS $$
  SELECT jsonb_build_object(
    'advogado_id', id::text,
    'advogado_nome', nome
  )
  FROM public.advogados
  WHERE id = p_user_id
  LIMIT 1;
$$;

-- Example: get claims for a user (for use in Edge Function / JWT customizer)
-- SELECT public.get_advogado_claims(auth.uid());
