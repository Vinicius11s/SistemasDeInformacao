-- Agile360 – Row Level Security (RLS) para isolamento por tenant (advogado_id)
-- Executar no Supabase após aplicar as migrations do EF Core.
-- Em produção, a API deve setar app.current_advogado_id na sessão (via connection ou SET LOCAL).

-- clientes
ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_clientes ON clientes
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- processos
ALTER TABLE processos ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_processos ON processos
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- audiencias
ALTER TABLE audiencias ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_audiencias ON audiencias
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- prazos
ALTER TABLE prazos ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_prazos ON prazos
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- notas
ALTER TABLE notas ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_notas ON notas
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- audit_logs (Story 1.2.1: apenas o advogado dono pode ler/escrever seus logs)
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_audit_logs ON audit_logs
  FOR ALL
  USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid)
  WITH CHECK (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- entradas_ia
ALTER TABLE entradas_ia ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_entradas_ia ON entradas_ia
  FOR ALL USING (advogado_id = current_setting('app.current_advogado_id', true)::uuid);

-- Nota: advogados não tem RLS (tabela de tenant root). Acesso a advogados deve ser controlado pela API (JWT).
