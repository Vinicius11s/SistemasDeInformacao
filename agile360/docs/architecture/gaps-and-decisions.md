# Agile360 – Gaps & Decisões (Tabela de Verificação)

**Versão:** 1.0.0  
**Criado por:** Aria (@architect)  
**Data:** 2026-02-20

Este documento registra gaps identificados na fundação do projeto e as decisões/recomendações tomadas para cada um.

---

## 📋 Tabela de Verificação de Gaps

| Item | Status Atual | Recomendação | Onde Foi Tratado |
|------|--------------|--------------|------------------|
| **Base de testes (Unit + Integration)** | Não existia | Setup xUnit + NSubstitute; testes de isolamento Multi-Tenancy (Query Filters) | **Story 1.1.1** – Test Foundation |
| **Camada de integração com IA (AI Gateway)** | Não definido | Autenticar webhooks n8n; HttpClient resiliente (Polly/Refit); interface padronizada para agentes | **Story 1.4** – Integration Foundation |
| **CI/CD e deployment** | Apenas Docker na 1.1 | GitHub Actions: build, test, Docker build/push; caminho para Azure/Cloudfy | **Story 1.5** – CI/CD Pipeline |
| **Auditoria sistêmica (Audit Trail)** | Apenas audit de auth na 1.3 | Shadow properties (CreatedBy/LastModifiedBy); tabela audit_logs para Processo/Prazo | **Story 1.2.1** – Audit Trail Foundation |
| **Documentação de API para Frontend** | Swagger na 1.1 | Gerar **Cliente TypeScript** a partir do OpenAPI (NSwag ou OpenAPI Generator) | **Story 1.1** – Task 2.6 |
| **Storage de documentos** | Não detalhado no schema | Definir **Supabase Storage** para PDFs de processos; bucket por tenant ou prefixo por advogado_id | **Decisão abaixo** + Story futura (Epic 2 ou 7) |
| **Mensageria / Background jobs** | MediatR configurado | Avaliar **Hangfire** para Epic 5 (Guardião de Prazos) – jobs agendados além do n8n | **Decisão abaixo** + Epic 5 |
| **DTO obrigatório na API** | Implícito nas stories | Nenhuma entidade EF Core exposta na API; sempre DTOs da Application | **Revalidação** + system-architecture |
| **Validator por Command que altera estado** | FluentValidation na 1.1 | Todo Command que altera estado deve ter Validator registrado; 400 antes da persistência | **Revalidação** + system-architecture |

---

## 🔧 Decisões Detalhadas

### 1. Base de Testes (Story 1.1.1)

- **Problema:** Em um CRM jurídico, um erro pode significar prazo perdido; sem testes, Multi-Tenancy e Query Filters podem “vazar” dados entre advogados.
- **Decisão:** Story dedicada 1.1.1 com xUnit, NSubstitute, FluentAssertions, WebApplicationFactory, e **testes explícitos de isolamento** (nenhum dado de Advogado B visível para Advogado A; insert automático de AdvogadoId).
- **Referência:** `docs/stories/story-1.1.1.md`

---

### 2. AI Gateway / Integração n8n (Story 1.4)

- **Problema:** Backend precisa de forma padronizada de autenticar e receber webhooks do n8n e de disparar chamadas resilientes a APIs de IA.
- **Decisão:**
  - Webhooks: autenticação por HMAC ou header secreto em rotas `/api/webhooks/*`.
  - Outbound: HttpClient nomeado com Polly (retry, circuit breaker, timeout); opcional Refit para contratos tipados.
  - Abstração `IAiGatewayService` para chamadas a n8n/IA (e futuramente AIOS).
- **Referência:** `docs/stories/story-1.4.md`, `docs/architecture/system-architecture.md` (seção Integration Foundation).

---

### 3. CI/CD (Story 1.5)

- **Problema:** Deploy manual via Docker é insustentável para SaaS; desejo de subir na nuvem (Cloudfy/Azure).
- **Decisão:** GitHub Actions com (1) CI: build + `dotnet test`; (2) CD: Docker build + push para registry. Documentar caminho para deploy (Azure Container Apps, Cloudfy, env vars). Não implementar deploy automático para produção nesta story.
- **Referência:** `docs/stories/story-1.5.md`, `docs/architecture/deployment.md` (a ser criado na 1.5).

---

### 4. Auditoria (Story 1.2.1)

- **Problema:** Em sistemas jurídicos, toda alteração em Processo/Prazo deve ser rastreada (quem criou/alterou).
- **Decisão:**
  - Shadow properties no EF Core: `CreatedBy`, `LastModifiedBy` (Guid?, preenchidos por interceptor a partir do usuário corrente).
  - Tabela `audit_logs` (entity_name, entity_id, action, advogado_id, old_values, new_values, changed_at) para histórico; RLS para isolamento por tenant.
- **Referência:** `docs/stories/story-1.2.1.md`

---

### 5. Cliente TypeScript (Story 1.1 – Task 2.6)

- **Problema:** Swagger está na 1.1, mas o frontend precisa de tipos alinhados ao contrato da API.
- **Decisão:** Adicionar à Story 1.1 a tarefa 2.6: configurar geração de cliente TypeScript a partir do OpenAPI (NSwag ou openapi-generator) em build ou script; output em `frontend/src/api/generated/` (ou documentar para o time de frontend).
- **Referência:** `docs/stories/story-1.1.md` – Phase 2, Task 2.6.

---

### 6. Storage de Documentos (PDFs de Processos)

- **Problema:** Onde e como armazenar arquivos de processos (petições, intimações em PDF)?
- **Decisão:**
  - **Onde:** **Supabase Storage** (já na stack).
  - **Estrutura sugerida:** Bucket `process-documents` (ou por tenant: bucket por advogado_id). Path sugerido: `{advogado_id}/{processo_id}/{nome_arquivo}` ou `{advogado_id}/processos/{processo_id}/{tipo}/{timestamp}_{nome}`.
  - **Segurança:** Políticas de Storage no Supabase restritas por `advogado_id` (RLS equivalente para storage).
  - **Story futura:** Incluir em Epic 2 (CRM) ou Epic 7 (Frontend) uma story “Upload e gestão de documentos de processo (Supabase Storage)” com definição final do bucket e políticas.
- **Referência:** Este documento; schema de Processo pode ganhar campo `Documentos` (lista de URLs ou entidade `ProcessoDocumento`) na story de documentos.

---

### 7. Mensageria / Background Jobs (Guardião de Prazos – Epic 5)

- **Problema:** MediatR está configurado; Epic 5 (Guardião de Prazos) precisa de execução agendada (verificar prazos periodicamente). n8n pode fazer cron, mas pode ser desejável ter jobs dentro da própria API (.NET).
- **Decisão:**
  - **Fase 1 (Epic 5):** Manter **n8n como orquestrador** do “Guardião de Prazos” (cron no n8n chama a API para listar prazos e envia alertas via Evolution API). Sem Hangfire inicialmente.
  - **Avaliação futura:** Se houver necessidade de jobs pesados dentro da API (ex.: relatórios, limpeza, sincronização em massa), avaliar **Hangfire** (ou equivalente) em uma story de “Background Jobs Foundation”. Até lá, n8n + API stateless é a estratégia recomendada.
- **Referência:** Epic 5 (Stories 5.1–5.3); `docs/architecture/system-architecture.md` (fluxo Guardião de Prazos).

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial gaps table and decisions | @architect (Aria) |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20
