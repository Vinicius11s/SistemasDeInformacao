-- ============================================================
-- RPC: get_dashboard_resumo
-- Agile360 — Dashboard Principal
-- ============================================================
-- Executa no SQL Editor do Supabase (ou via migration).
--
-- Retorna em um único objeto JSON:
--   • contadores        → 4 cards do topo
--   • compromissos_semana → calendário semanal (próximos 7 dias)
--   • processos_recentes  → lista dos 5 processos mais recentes
--
-- Segurança:
--   • SECURITY INVOKER (padrão): executa com as permissões do
--     usuário que chamou a função → RLS é respeitado automaticamente.
--   • Nenhum dado de outro advogado é retornado.
-- ============================================================

CREATE OR REPLACE FUNCTION get_dashboard_resumo()
RETURNS json
LANGUAGE plpgsql
SECURITY INVOKER
AS $$
DECLARE
  v_hoje         DATE := CURRENT_DATE;
  v_fim3dias     DATE := CURRENT_DATE + INTERVAL '3 days';
  v_inicio_mes   DATE := DATE_TRUNC('month', CURRENT_DATE)::DATE;
  v_inicio_semana DATE := DATE_TRUNC('week', CURRENT_DATE)::DATE;
  v_fim_semana   DATE := (DATE_TRUNC('week', CURRENT_DATE) + INTERVAL '6 days')::DATE;

  v_audiencias_hoje    INT;
  v_atendimentos_hoje  INT;
  v_prazos_fatais      INT;
  v_novos_processos    INT;

BEGIN
  -- ── Contadores ──────────────────────────────────────────────
  SELECT
    COUNT(*) FILTER (WHERE tipo_compromisso = 'Audiência'),
    COUNT(*) FILTER (WHERE tipo_compromisso = 'Atendimento')
  INTO v_audiencias_hoje, v_atendimentos_hoje
  FROM compromisso
  WHERE data = v_hoje;                -- RLS filtra por id_advogado

  SELECT COUNT(*)
  INTO v_prazos_fatais
  FROM compromisso
  WHERE tipo_compromisso = 'Prazo'
    AND data BETWEEN v_hoje AND v_fim3dias;

  SELECT COUNT(*)
  INTO v_novos_processos
  FROM processo
  WHERE criado_em >= v_inicio_mes;    -- RLS filtra por id_advogado

  -- ── Retorno único ────────────────────────────────────────────
  RETURN json_build_object(
    'contadores', json_build_object(
      'audiencias_hoje',    v_audiencias_hoje,
      'atendimentos_hoje',  v_atendimentos_hoje,
      'prazos_fatais',      v_prazos_fatais,
      'novos_processos_mes', v_novos_processos
    ),

    'compromissos_semana', (
      SELECT json_agg(
        json_build_object(
          'id',              id,
          'tipo',            tipo_compromisso,
          'status',          status,
          'data',            data,
          'hora',            TO_CHAR(hora, 'HH24:MI'),
          'local',           local,
          'id_processo',     id_processo
        ) ORDER BY data ASC, hora ASC
      )
      FROM compromisso
      WHERE data BETWEEN v_inicio_semana AND v_fim_semana
    ),

    'processos_recentes', (
      SELECT json_agg(sub)
      FROM (
        SELECT
          id,
          num_processo,
          status,
          assunto,
          tribunal,
          criado_em
        FROM processo
        ORDER BY criado_em DESC NULLS LAST
        LIMIT 5
      ) sub
    )
  );
END;
$$;

-- Permissão para usuários autenticados chamarem a RPC
GRANT EXECUTE ON FUNCTION get_dashboard_resumo() TO authenticated;

-- ============================================================
-- Como usar via Supabase PostgREST:
--   POST /rest/v1/rpc/get_dashboard_resumo
--   Authorization: Bearer <access_token>
--   Content-Type: application/json
--   Body: {}
-- ============================================================
