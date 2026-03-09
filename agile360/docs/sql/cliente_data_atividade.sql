-- ============================================================
-- Agile360 – Adicionar data_referencia e atividade em clientes
--
-- Semântica por TipoPessoa:
--   PessoaFisica   → data_referencia = data de nascimento
--                    atividade       = profissão
--   PessoaJuridica → data_referencia = data de abertura/fundação
--                    atividade       = ramo de atividade
--
-- Execute no SQL Editor do Supabase como postgres/service_role
-- ============================================================

ALTER TABLE public.clientes
    ADD COLUMN IF NOT EXISTS data_referencia date,
    ADD COLUMN IF NOT EXISTS area_atuacao    varchar(200);

-- Índice útil para filtros de relatório por período (aniversários, vencimentos)
CREATE INDEX IF NOT EXISTS idx_clientes_data_referencia
    ON public.clientes (advogado_id, data_referencia)
    WHERE data_referencia IS NOT NULL;

COMMENT ON COLUMN public.clientes.data_referencia IS
    'PessoaFisica: data de nascimento | PessoaJuridica: data de abertura/fundação';

COMMENT ON COLUMN public.clientes.area_atuacao IS
    'PessoaFisica: profissão | PessoaJuridica: ramo de atividade';
