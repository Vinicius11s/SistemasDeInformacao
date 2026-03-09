-- Agile360 – Seed de demonstração (opcional)
-- Executar após migrations e RLS. Ajustar IDs conforme necessário.
-- Story 1.2 Phase 5: DatabaseSeeder em C# pode popular via aplicação; este script é alternativa.

-- Exemplo: 1 advogado demo (substituir ID pelo real após criar no Supabase Auth na Story 1.3)
-- INSERT INTO advogados (id, nome, email, oab, ativo, created_at, updated_at)
-- VALUES (
--   '00000000-0000-0000-0000-000000000001',
--   'Advogado Demo',
--   'demo@agile360.com',
--   'OAB/SP 123456',
--   true,
--   NOW(),
--   NOW()
-- );

-- Clientes, Processos, Audiências e Prazos de exemplo devem ser inseridos
-- com advogado_id do advogado demo. Ver DatabaseSeeder na aplicação (Story 1.2 Phase 5).
