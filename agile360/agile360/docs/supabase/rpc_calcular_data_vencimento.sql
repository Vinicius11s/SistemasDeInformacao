-- ═══════════════════════════════════════════════════════════════════════════
--  Agile360 — RPC: calcular_data_vencimento
--
--  Calcula a data_vencimento de um prazo com base em:
--    p_data_publicacao : data de início da contagem
--    p_prazo_dias      : quantidade de dias do prazo
--    p_tipo_contagem   : 'Util' (dias úteis) ou 'Corrido' (dias corridos)
--
--  Regras:
--    - Tipo 'Corrido': soma direto os dias ao calendário.
--    - Tipo 'Util'   : pula sábados (DOW=6) e domingos (DOW=0).
--    - Se o resultado final cair em fim de semana, avança para segunda-feira.
--
--  Uso:
--    SELECT calcular_data_vencimento('2026-02-26', 15, 'Util');
-- ═══════════════════════════════════════════════════════════════════════════

CREATE OR REPLACE FUNCTION public.calcular_data_vencimento(
    p_data_publicacao DATE,
    p_prazo_dias      INTEGER,
    p_tipo_contagem   TEXT DEFAULT 'Util'
)
RETURNS DATE
LANGUAGE plpgsql
IMMUTABLE
AS $$
DECLARE
    v_data          DATE    := p_data_publicacao;
    v_dias_contados INTEGER := 0;
BEGIN
    -- Validações básicas
    IF p_data_publicacao IS NULL THEN
        RAISE EXCEPTION 'data_publicacao não pode ser nula';
    END IF;
    IF p_prazo_dias IS NULL OR p_prazo_dias <= 0 THEN
        RAISE EXCEPTION 'prazo_dias deve ser um inteiro positivo';
    END IF;

    -- ── Dias Corridos ─────────────────────────────────────────────────────
    IF UPPER(p_tipo_contagem) = 'CORRIDO' THEN
        v_data := p_data_publicacao + p_prazo_dias;
    ELSE
    -- ── Dias Úteis ────────────────────────────────────────────────────────
    -- Avança dia a dia, contando apenas Seg–Sex
        WHILE v_dias_contados < p_prazo_dias LOOP
            v_data := v_data + 1;
            -- DOW: 0=Domingo, 6=Sábado
            IF EXTRACT(DOW FROM v_data) NOT IN (0, 6) THEN
                v_dias_contados := v_dias_contados + 1;
            END IF;
        END LOOP;
    END IF;

    -- ── Ajuste final: se caiu em fim de semana, avança para segunda ───────
    IF EXTRACT(DOW FROM v_data) = 6 THEN   -- Sábado → +2
        v_data := v_data + 2;
    ELSIF EXTRACT(DOW FROM v_data) = 0 THEN -- Domingo → +1
        v_data := v_data + 1;
    END IF;

    RETURN v_data;
END;
$$;

-- Permissão: authenticated users podem chamar esta função
GRANT EXECUTE ON FUNCTION public.calcular_data_vencimento(DATE, INTEGER, TEXT)
  TO authenticated;

-- ─── Exemplos de uso ─────────────────────────────────────────────────────────
-- 15 dias úteis a partir de uma quinta-feira
-- SELECT calcular_data_vencimento('2026-02-26', 15, 'Util');   → 2026-03-19 (Sex)
-- SELECT calcular_data_vencimento('2026-02-26', 15, 'Corrido'); → 2026-03-13 (Sex)
-- Quando o resultado cairia num sábado, vira segunda
-- SELECT calcular_data_vencimento('2026-02-26', 7, 'Util');     → resultado ajustado
