# Story 1.2: Multi-Tenancy Architecture & Database Schema

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.2
**Sprint:** 1
**Priority:** 🔴 Critical
**Points:** 13
**Effort:** 10-16 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| Multi-Tenancy por advogado_id | PRD Agile360 | Todas as entidades terão `advogado_id` como tenant key |
| Supabase PostgreSQL | PRD Agile360 | RLS (Row Level Security) como mecanismo de isolamento |
| EF Core como ORM | Story 1.1 | Migrations e configurações via Fluent API |
| BaseEntity padrão | Story 1.1 | Herdar `Id`, `CreatedAt`, `UpdatedAt`, `IsActive`, `AdvogadoId` |

---

## 📋 User Story

**Como** advogado utilizando o Agile360,
**Quero** que meus dados (clientes, processos, audiências) estejam completamente isolados de outros advogados,
**Para** ter segurança jurídica e privacidade total das informações dos meus clientes.

---

## 🎯 Objective

Implementar a arquitetura Multi-Tenancy do Agile360 utilizando isolamento por `advogado_id` no nível de banco de dados, com Row Level Security (RLS) do PostgreSQL via Supabase. Criar o schema completo do banco de dados com todas as entidades do CRM jurídico (Advogado, Cliente, Processo, Audiência, Prazo, Documento) e configurar as migrations do EF Core.

---

## ✅ Tasks

### Phase 1: Domain Entities (~3h)

- [ ] **1.1** Criar entity `Advogado`:
  ```
  - Id (Guid, PK)
  - Nome (string, required, max 200)
  - Email (string, required, unique)
  - OAB (string, required, unique) – Número da OAB
  - Telefone (string, nullable)
  - WhatsAppId (string, nullable) – ID Evolution API
  - FotoUrl (string, nullable)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.2** Criar entity `Cliente`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - Nome (string, required, max 200)
  - CPF (string, nullable, max 14)
  - RG (string, nullable, max 20)
  - Email (string, nullable)
  - Telefone (string, nullable)
  - WhatsAppNumero (string, nullable)
  - Endereco (string, nullable)
  - Observacoes (text, nullable)
  - Origem (enum: Manual, WhatsApp, Email, Indicacao)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.3** Criar entity `Processo`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - ClienteId (Guid, FK → Cliente)
  - NumeroProcesso (string, required, max 30)
  - Vara (string, nullable, max 100)
  - Comarca (string, nullable, max 100)
  - Tribunal (string, nullable, max 100)
  - TipoAcao (string, nullable, max 200)
  - ValorCausa (decimal, nullable)
  - Status (enum: Ativo, Arquivado, Suspenso, Encerrado)
  - Descricao (text, nullable)
  - DataDistribuicao (DateOnly, nullable)
  - UltimaMovimentacao (DateTime, nullable)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.4** Criar entity `Audiencia`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - ProcessoId (Guid, FK → Processo)
  - DataHora (DateTimeOffset, required)
  - Local (string, nullable, max 300)
  - Tipo (enum: Conciliacao, Instrucao, Julgamento, Virtual, Outra)
  - Status (enum: Agendada, Realizada, Adiada, Cancelada)
  - Observacoes (text, nullable)
  - GoogleEventId (string, nullable) – Sync com Google Agenda
  - LembretesEnviados (int, default 0)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.5** Criar entity `Prazo`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - ProcessoId (Guid, FK → Processo)
  - Descricao (string, required, max 500)
  - DataVencimento (DateTimeOffset, required)
  - Tipo (enum: Fatal, Ordinario, Diligencia)
  - Prioridade (enum: Critica, Alta, Media, Baixa)
  - Status (enum: Pendente, Cumprido, Expirado, Cancelado)
  - AlertaEnviado (bool, default false)
  - OrigemIntimacao (string, nullable) – ID do e-mail de origem
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.6** Criar entity `Nota`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - ProcessoId (Guid, FK → Processo, nullable) – Pode ser nota geral
  - Titulo (string, required, max 200)
  - Conteudo (text, required)
  - Fixada (bool, default false)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.7** Criar entity `EntradaIA`:
  ```
  - Id (Guid, PK)
  - AdvogadoId (Guid, FK → Advogado) – Tenant Key
  - Origem (enum: WhatsApp, Email)
  - ConteudoOriginal (text, required) – Mensagem bruta
  - DadosExtraidos (jsonb, nullable) – JSON com dados parseados pela IA
  - ClienteId (Guid, FK → Cliente, nullable) – Se já vinculado
  - ProcessoId (Guid, FK → Processo, nullable)
  - Status (enum: Pendente, Processado, Descartado, Erro)
  - ProcessadoEm (DateTime, nullable)
  - Ativo (bool, default true)
  - CreatedAt, UpdatedAt
  ```
- [ ] **1.8** Criar Enums em `Domain/Enums/`:
  - `OrigemCliente`, `StatusProcesso`, `TipoAudiencia`, `StatusAudiencia`
  - `TipoPrazo`, `PrioridadePrazo`, `StatusPrazo`
  - `OrigemEntradaIA`, `StatusEntradaIA`

### Phase 2: EF Core Configuration (~3h)

- [ ] **2.1** Criar `AdvogadoConfiguration` (Fluent API):
  - Unique index em `Email`
  - Unique index em `OAB`
  - Required fields validation
- [ ] **2.2** Criar `ClienteConfiguration`:
  - FK para `Advogado` com cascade delete restrict
  - Index em `AdvogadoId`
  - Index em `CPF` (filtrado por tenant)
- [ ] **2.3** Criar `ProcessoConfiguration`:
  - FK para `Advogado` e `Cliente`
  - Index em `NumeroProcesso` (unique por tenant)
  - Index composto `(AdvogadoId, Status)`
- [ ] **2.4** Criar `AudienciaConfiguration`:
  - FK para `Advogado` e `Processo`
  - Index em `DataHora` para queries de calendário
  - Index composto `(AdvogadoId, DataHora, Status)`
- [ ] **2.5** Criar `PrazoConfiguration`:
  - FK para `Advogado` e `Processo`
  - Index em `DataVencimento`
  - Index composto `(AdvogadoId, DataVencimento, Status)`
- [ ] **2.6** Criar `NotaConfiguration` e `EntradaIAConfiguration`
- [ ] **2.7** Registrar todas as entities no `Agile360DbContext`
- [ ] **2.8** Criar e executar a migration inicial: `InitialSchema`

### Phase 3: Multi-Tenancy Implementation (~3h)

- [ ] **3.1** Criar `ITenantProvider` interface:
  ```csharp
  public interface ITenantProvider {
      Guid GetCurrentAdvogadoId();
      void SetCurrentAdvogadoId(Guid advogadoId);
  }
  ```
- [ ] **3.2** Implementar `TenantProvider` (resolve via HttpContext/JWT claims)
- [ ] **3.3** Criar `TenantMiddleware`:
  - Extrai `advogado_id` do JWT claim
  - Seta no `ITenantProvider` (scoped per request)
  - Valida que o advogado existe e está ativo
- [ ] **3.4** Implementar Global Query Filter no DbContext:
  ```csharp
  modelBuilder.Entity<Cliente>()
      .HasQueryFilter(c => c.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
  ```
  - Aplicar para: Cliente, Processo, Audiência, Prazo, Nota, EntradaIA
- [ ] **3.5** Criar `TenantSaveChangesInterceptor`:
  - Auto-set `AdvogadoId` em novas entities
  - Validar que update/delete não altera entities de outro tenant
- [ ] **3.6** Criar SQL para Row Level Security (RLS) no Supabase:
  ```sql
  ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;
  CREATE POLICY tenant_isolation ON clientes
    USING (advogado_id = current_setting('app.current_advogado_id')::uuid);
  ```
  - Aplicar RLS em todas as tabelas tenant-aware

### Phase 4: Repositories Especializados (~2h)

- [ ] **4.1** Criar `IClienteRepository` extends `IRepository<Cliente>`:
  - `GetByWhatsAppAsync(string numero)`
  - `GetByCpfAsync(string cpf)`
  - `SearchAsync(string termo)` – busca por nome, CPF, telefone
- [ ] **4.2** Criar `IProcessoRepository` extends `IRepository<Processo>`:
  - `GetByNumeroAsync(string numeroProcesso)`
  - `GetRecentesAsync(int count)` – últimos processos acessados
  - `GetByStatusAsync(StatusProcesso status)`
- [ ] **4.3** Criar `IAudienciaRepository` extends `IRepository<Audiencia>`:
  - `GetHojeAsync()` – audiências de hoje
  - `GetSemanaAsync()` – audiências da semana
  - `GetProximaAsync()` – próxima audiência
- [ ] **4.4** Criar `IPrazoRepository` extends `IRepository<Prazo>`:
  - `GetVencimentoProximoAsync(int horasAntes)` – vencendo em X horas
  - `GetPendentesAsync()` – prazos pendentes
  - `GetFataisAsync()` – prazos fatais pendentes
- [ ] **4.5** Implementar todos os repositories em `Infrastructure/Repositories/`

### Phase 5: Seed Data & Validation (~1h)

- [ ] **5.1** Criar `DatabaseSeeder` com dados de demonstração:
  - 1 Advogado demo
  - 5 Clientes com dados variados
  - 3 Processos com números reais formatados
  - 5 Audiências (passadas e futuras)
  - 4 Prazos (incluindo 1 fatal próximo)
- [ ] **5.2** Criar script SQL de seed para Supabase
- [ ] **5.3** Validar todas as migrations no Supabase

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN o schema do banco de dados está migrado
WHEN uma query é feita para listar todas as tabelas
THEN as tabelas advogados, clientes, processos, audiencias, prazos, notas, entradas_ia existem
AND todas possuem os campos definidos no schema

GIVEN o Multi-Tenancy está configurado
WHEN o Advogado A cria um cliente
THEN o cliente recebe automaticamente o advogado_id do Advogado A
AND o Advogado B não consegue ver esse cliente em nenhuma query

GIVEN o Global Query Filter está ativo
WHEN uma query é feita sem filtro explícito de advogado_id
THEN o EF Core aplica automaticamente o filtro do tenant corrente
AND apenas dados do advogado logado são retornados

GIVEN o RLS está habilitado no Supabase
WHEN uma query direta ao PostgreSQL é feita
THEN apenas registros do tenant corrente são acessíveis
AND tentativas de acessar dados de outro tenant são bloqueadas

GIVEN o seed data está inserido
WHEN o advogado demo acessa o sistema
THEN vê 5 clientes, 3 processos, 5 audiências e 4 prazos
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Database schema e multi-tenancy são infraestrutura |
| Complexity | High | RLS, query filters, interceptors, múltiplas entities |
| Test Requirements | Integration + Unit | Testar isolamento de dados, queries filtradas |
| Review Focus | Security, Architecture | Multi-tenancy é crítico para segurança de dados |

### Agent Assignment

| Role | Agent | Responsibility |
|------|-------|----------------|
| Primary | @dev | Implementação do schema e multi-tenancy |
| Secondary | @db-sage | Validação do schema e RLS policies |
| Review | @architect | Validação da arquitetura multi-tenant |

### Self-Healing Config

```yaml
reviews:
  auto_review:
    enabled: true
    drafts: false
  path_instructions:
    - path: "src/Agile360.Domain/Entities/**"
      instructions: "Verificar que todas entities herdam BaseEntity e possuem AdvogadoId"
    - path: "src/Agile360.Infrastructure/Data/**"
      instructions: "Verificar Global Query Filters, RLS scripts, cascade behaviors"
    - path: "src/Agile360.Infrastructure/Repositories/**"
      instructions: "Verificar que queries respeitam tenant isolation"

chat:
  auto_reply: true
```

### Focus Areas

- [ ] Tenant Isolation: Nenhum data leak entre advogados
- [ ] RLS Policies: PostgreSQL level security configurado
- [ ] Query Filters: EF Core filtrando automaticamente por tenant
- [ ] Cascade Delete: Configuração correta para evitar orphan records
- [ ] Indexes: Performance indexes para queries frequentes

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (DbContext, BaseEntity, Repository pattern)

**Blocks:**
- Story 1.2.1: Audit Trail Foundation (shadow properties em entidades desta story)
- Story 1.3: Authentication & Authorization (precisa do Advogado entity)
- Story 2.1-2.4: CRM CRUD (precisa das entities e repositories)
- Story 3.1-3.4: WhatsApp Integration (precisa de Cliente, EntradaIA)
- Story 4.1-4.3: Monitor de Intimações (precisa de Processo, Audiência, Prazo)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Data leak entre tenants | Critical | Dupla proteção: EF Query Filters + PostgreSQL RLS |
| Performance com Global Query Filters | Medium | Indexes otimizados para queries com advogado_id |
| EF Core + RLS conflito | Medium | Testar compatibilidade antes de implementar ambos |
| Migration falha no Supabase | Medium | Testar migrations em ambiente de dev primeiro |
| Schema lock durante migration | Low | Executar migrations em horário de baixo uso |

---

## 📋 Definition of Done

- [ ] Todas as alterações de schema do banco são feitas exclusivamente via Migrations do EF Core (nunca alterar banco em produção sem migration correspondente)
- [ ] Todas as 7 entities criadas com campos corretos
- [ ] Enums de domínio implementados
- [ ] EF Core Fluent API configurada para todas entities
- [ ] Migration `InitialSchema` criada e aplicada no Supabase
- [ ] Indexes criados para queries de alta frequência
- [ ] Multi-Tenancy via Global Query Filter funcionando
- [ ] TenantMiddleware extraindo advogado_id do JWT
- [ ] TenantSaveChangesInterceptor auto-setando advogado_id
- [ ] RLS policies criadas no PostgreSQL/Supabase
- [ ] Repositories especializados implementados
- [ ] Seed data inserido com dados de demonstração
- [ ] Zero data leak entre tenants (validado por testes)
- [ ] All acceptance criteria verified
- [ ] Tests passing
- [ ] Documentation updated

---

## 📝 Dev Notes

### Schema extensions (Story 1.2.1)

As tabelas **processos**, **prazos**, **clientes**, **audiencias** e **notas** recebem colunas de auditoria (shadow properties): `CreatedBy` e `LastModifiedBy` (uuid, nullable). A tabela **audit_logs** armazena o histórico de alterações (entity_name, entity_id, action, advogado_id, old_values, new_values, changed_at) para Processo e Prazo. Ver [Story 1.2.1](story-1.2.1.md) e [System Architecture – Audit Trail](../architecture/system-architecture.md#audit-trail-story-121).

### Key Files

```
src/
├── Agile360.Domain/
│   ├── Entities/
│   │   ├── BaseEntity.cs (update from 1.1)
│   │   ├── Advogado.cs
│   │   ├── Cliente.cs
│   │   ├── Processo.cs
│   │   ├── Audiencia.cs
│   │   ├── Prazo.cs
│   │   ├── Nota.cs
│   │   └── EntradaIA.cs
│   ├── Enums/
│   │   ├── OrigemCliente.cs
│   │   ├── StatusProcesso.cs
│   │   ├── TipoAudiencia.cs
│   │   ├── StatusAudiencia.cs
│   │   ├── TipoPrazo.cs
│   │   ├── PrioridadePrazo.cs
│   │   ├── StatusPrazo.cs
│   │   ├── OrigemEntradaIA.cs
│   │   └── StatusEntradaIA.cs
│   └── Interfaces/
│       ├── ITenantProvider.cs
│       ├── IClienteRepository.cs
│       ├── IProcessoRepository.cs
│       ├── IAudienciaRepository.cs
│       └── IPrazoRepository.cs
│
├── Agile360.Infrastructure/
│   ├── Data/
│   │   ├── Agile360DbContext.cs (update)
│   │   ├── Configurations/
│   │   │   ├── AdvogadoConfiguration.cs
│   │   │   ├── ClienteConfiguration.cs
│   │   │   ├── ProcessoConfiguration.cs
│   │   │   ├── AudienciaConfiguration.cs
│   │   │   ├── PrazoConfiguration.cs
│   │   │   ├── NotaConfiguration.cs
│   │   │   └── EntradaIAConfiguration.cs
│   │   ├── Interceptors/
│   │   │   └── TenantSaveChangesInterceptor.cs
│   │   ├── Migrations/
│   │   │   └── YYYYMMDD_InitialSchema.cs
│   │   └── Seeders/
│   │       └── DatabaseSeeder.cs
│   ├── MultiTenancy/
│   │   └── TenantProvider.cs
│   └── Repositories/
│       ├── ClienteRepository.cs
│       ├── ProcessoRepository.cs
│       ├── AudienciaRepository.cs
│       └── PrazoRepository.cs
│
└── Agile360.API/
    └── Middleware/
        └── TenantMiddleware.cs

sql/
├── rls-policies.sql
└── seed-data.sql
```

### ERD (Entity Relationship Diagram)

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│  Advogado   │1    N │   Cliente   │1    N │  Processo   │
│─────────────│───────│─────────────│───────│─────────────│
│ Id (PK)     │       │ Id (PK)     │       │ Id (PK)     │
│ Nome        │       │ AdvogadoId  │       │ AdvogadoId  │
│ Email       │       │ Nome        │       │ ClienteId   │
│ OAB         │       │ CPF         │       │ Numero      │
│ Telefone    │       │ Telefone    │       │ Vara        │
│ WhatsAppId  │       │ WhatsApp    │       │ Status      │
└─────────────┘       │ Origem      │       └──────┬──────┘
       │              └─────────────┘              │
       │                                     1     │  N
       │              ┌─────────────┐       ┌──────┴──────┐
       │         N    │  Audiencia  │       │    Prazo    │
       ├──────────────│─────────────│       │─────────────│
       │              │ Id (PK)     │       │ Id (PK)     │
       │              │ AdvogadoId  │       │ AdvogadoId  │
       │              │ ProcessoId  │       │ ProcessoId  │
       │              │ DataHora    │       │ Vencimento  │
       │              │ Tipo        │       │ Tipo        │
       │              │ Status      │       │ Prioridade  │
       │              └─────────────┘       │ Status      │
       │                                    └─────────────┘
       │              ┌─────────────┐       ┌─────────────┐
       │         N    │    Nota     │  N    │ EntradaIA   │
       ├──────────────│─────────────│───────│─────────────│
                      │ Id (PK)     │       │ Id (PK)     │
                      │ AdvogadoId  │       │ AdvogadoId  │
                      │ ProcessoId? │       │ Origem      │
                      │ Titulo      │       │ Conteudo    │
                      │ Conteudo    │       │ DadosJSON   │
                      │ Fixada      │       │ Status      │
                      └─────────────┘       └─────────────┘
```

### Interface Segregation (ISP)

- Ao criar novos repositórios ou serviços, preferir interfaces específicas por agregado/use case.
- Não estender uma interface genérica com dezenas de métodos não relacionados.

### Testing Checklist

#### Multi-Tenancy Isolation
- [ ] Advogado A não vê clientes do Advogado B
- [ ] Advogado A não vê processos do Advogado B
- [ ] Query sem tenant filter retorna apenas dados do tenant corrente
- [ ] Insert automático de advogado_id em novas entities
- [ ] Update/Delete bloqueado para entities de outro tenant

#### Database Schema
- [ ] Migration aplica sem erros no Supabase
- [ ] Todos os indexes criados corretamente
- [ ] Foreign keys com cascade behavior correto
- [ ] Enums mapeados corretamente
- [ ] Campos nullable/required conforme especificação

#### RLS Policies
- [ ] RLS habilitado em todas as tabelas tenant-aware
- [ ] Policy permite apenas leitura do próprio tenant
- [ ] Policy permite apenas escrita no próprio tenant
- [ ] Superuser bypass funciona para operações administrativas

---

## 🧑‍💻 Dev Agent Record

> This section is populated when @dev executes the story.

### Execution Log

| Timestamp | Phase | Action | Result |
|-----------|-------|--------|--------|
| - | - | Awaiting execution | - |

### Implementation Notes

_To be filled during execution._

### Issues Encountered

_None yet - story not started._

---

## 🧪 QA Results

> This section is populated after @qa reviews the implementation.

### Test Execution Summary

| Category | Tests | Passed | Failed | Skipped |
|----------|-------|--------|--------|---------|
| Unit | - | - | - | - |
| Integration | - | - | - | - |
| E2E | - | - | - | - |

### Validation Checklist

| Check | Status | Notes |
|-------|--------|-------|
| Acceptance criteria | ⏳ | |
| DoD items | ⏳ | |
| Edge cases | ⏳ | |
| Documentation | ⏳ | |

### QA Sign-off

- [ ] All acceptance criteria verified
- [ ] Tests passing (coverage ≥80%)
- [ ] Documentation complete
- [ ] Ready for release

**QA Agent:** _Awaiting assignment_
**Date:** _Pending_

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story creation | @architect (Aria) |

---

**Criado por:** Aria (@architect)
**Data:** 2026-02-20
**Atualizado:** 2026-02-20 (Initial creation)
