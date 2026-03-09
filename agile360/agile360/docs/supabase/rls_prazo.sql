-- ═══════════════════════════════════════════════════════════════════════════
--  Agile360 — RLS para a tabela public.prazo
--  Execute este script no SQL Editor do Supabase APÓS criar a tabela.
--
--  Filosofia: cada advogado só vê e manipula os seus próprios prazos.
--  O auth.uid() retorna o UUID do usuário autenticado pelo GoTrue.
-- ═══════════════════════════════════════════════════════════════════════════

-- 1. Habilitar RLS na tabela
ALTER TABLE public.prazo ENABLE ROW LEVEL SECURITY;

-- 2. Remover políticas antigas (idempotente)
DROP POLICY IF EXISTS "prazo_select"  ON public.prazo;
DROP POLICY IF EXISTS "prazo_insert"  ON public.prazo;
DROP POLICY IF EXISTS "prazo_update"  ON public.prazo;
DROP POLICY IF EXISTS "prazo_delete"  ON public.prazo;

-- 3. SELECT — advogado lê apenas seus prazos
CREATE POLICY "prazo_select"
  ON public.prazo
  FOR SELECT
  USING (id_advogado = auth.uid());

-- 4. INSERT — advogado só insere com seu próprio id
CREATE POLICY "prazo_insert"
  ON public.prazo
  FOR INSERT
  WITH CHECK (id_advogado = auth.uid());

-- 5. UPDATE — advogado só atualiza seus prazos
CREATE POLICY "prazo_update"
  ON public.prazo
  FOR UPDATE
  USING     (id_advogado = auth.uid())
  WITH CHECK (id_advogado = auth.uid());

-- 6. DELETE — advogado só exclui seus prazos
CREATE POLICY "prazo_delete"
  ON public.prazo
  FOR DELETE
  USING (id_advogado = auth.uid());

-- ─── Índices de performance ──────────────────────────────────────────────────
-- Consultas frequentes: listar pendentes ordenados por vencimento
CREATE INDEX IF NOT EXISTS prazo_advogado_status_vencimento_idx
  ON public.prazo (id_advogado, status, data_vencimento ASC);

-- Consultas por cliente
CREATE INDEX IF NOT EXISTS prazo_cliente_idx
  ON public.prazo (id_cliente);

-- Consultas por processo
CREATE INDEX IF NOT EXISTS prazo_processo_idx
  ON public.prazo (id_processo)
  WHERE id_processo IS NOT NULL;
