-- Story 1.3: Trigger to create public.advogados row when a new user signs up in auth.users.
-- Run in Supabase SQL Editor (auth schema is available there).

-- Function: insert into advogados from auth.users (id, email, nome from raw_user_meta_data, optional oab/telefone)
CREATE OR REPLACE FUNCTION public.handle_new_auth_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
  INSERT INTO public.advogados (
    id,
    nome,
    email,
    oab,
    telefone,
    ativo,
    created_at,
    updated_at
  ) VALUES (
    NEW.id,
    COALESCE(NEW.raw_user_meta_data->>'nome', NEW.raw_user_meta_data->>'full_name', split_part(NEW.email, '@', 1), 'Usuário'),
    NEW.email,
    COALESCE(NEW.raw_user_meta_data->>'oab', ''),
    NEW.raw_user_meta_data->>'telefone',
    true,
    NOW(),
    NOW()
  );
  RETURN NEW;
END;
$$;

-- Trigger: after insert on auth.users
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW
  EXECUTE FUNCTION public.handle_new_auth_user();
