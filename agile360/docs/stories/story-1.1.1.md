# Story 1.1.1: Test Foundation – Unit & Integration (xUnit, NSubstitute)

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.1.1
**Sprint:** 1
**Priority:** 🔴 Critical
**Points:** 5
**Effort:** 4-6 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| CRM jurídico – erro = prazo perdido | PRD Agile360 | Base de testes é vital; priorizar testes de isolamento |
| Multi-Tenancy por advogado_id | Story 1.2 | Testes devem validar que Query Filters não vazam dados |
| Clean Architecture | Story 1.1 | Testes unitários em Application/Domain; integração em API/Infra |
| EF Core Global Query Filters | Story 1.2 | Testes de integração devem cobrir cenários de tenant isolation |

---

## 📋 User Story

**Como** desenvolvedor do Agile360,
**Quero** uma base sólida de testes de unidade e integração com xUnit e NSubstitute,
**Para** garantir que Multi-Tenancy e Query Filters nunca permitam vazamento de dados entre advogados e que regras de negócio críticas (prazos, processos) sejam cobertas por testes automatizados.

---

## 🎯 Objective

Configurar a infraestrutura de testes do projeto com xUnit (unit + integration), NSubstitute para mocks, e testes de integração com banco em memória ou container. Incluir testes explícitos que validem o isolamento multi-tenant (nenhum dado de Advogado B visível para Advogado A) e a aplicação correta dos Global Query Filters do EF Core.

---

## ✅ Tasks

### Phase 1: Test Projects Structure (~1h)

- [ ] **1.1** Criar projeto `Agile360.UnitTests`:
  - TargetFramework: net9.0
  - Referências: Agile360.Application, Agile360.Domain, Agile360.Shared
  - Pacotes: xUnit, xUnit.runner.visualstudio, NSubstitute, FluentAssertions, Microsoft.NET.Test.Sdk
- [ ] **1.2** Criar projeto `Agile360.IntegrationTests`:
  - TargetFramework: net9.0
  - Referências: Agile360.API (WebApplicationFactory), Agile360.Infrastructure, Agile360.Application
  - Pacotes: xUnit, Microsoft.AspNetCore.Mvc.Testing, Npgsql (ou Testcontainers para PostgreSQL)
- [ ] **1.3** Adicionar ambos à Solution (`Agile360.sln`)
- [ ] **1.4** Configurar `Directory.Build.props` (se não existir) para:
  - IsTestProject = true nos projetos de teste
  - Nullable = enable
  - TreatWarningsAsErrors em Release (opcional)
- [ ] **1.5** Criar estrutura de pastas padrão:
  - UnitTests: `Application/`, `Domain/`, `Shared/`
  - IntegrationTests: `Api/`, `Database/`, `MultiTenancy/`

### Phase 2: Unit Test Infrastructure (~1h)

- [ ] **2.1** Criar `TestBase` ou fixtures reutilizáveis:
  - Fixture para MediatR (configurar pipeline sem infra real)
  - Helpers para construir entidades de teste (Advogado, Cliente, Processo)
- [ ] **2.2** Criar builders/factories de teste:
  - `AdvogadoBuilder`, `ClienteBuilder`, `ProcessoBuilder` com valores padrão válidos
  - Métodos `.WithAdvogadoId(Guid)`, `.WithNome(string)` etc. para cenários específicos
- [ ] **2.3** Configurar NSubstitute para interfaces comuns:
  - Exemplo: `IRepository<Cliente>`, `IUnitOfWork`, `ITenantProvider`, `ICurrentUserService`
- [ ] **2.4** Escrever 2–3 testes unitários de exemplo (Application layer):
  - Ex.: validação de Command, comportamento de Handler com mock de repositório
- [ ] **2.5** Configurar cobertura (opcional): coverlet.collector, report generator
  - Meta: ≥80% em Application/Domain para novas features (documentar em DoD)

### Phase 3: Integration Test Infrastructure (~1.5h)

- [ ] **3.1** Criar `Agile360WebApplicationFactory` herdando `WebApplicationFactory<Program>`:
  - Override `ConfigureWebHost` para usar banco de testes (SQLite in-memory ou Testcontainers)
  - Substituir serviços externos (Supabase Auth, n8n) por mocks quando necessário
  - Garantir que `Program` seja acessível (InternalsVisibleTo ou assembly exposto)
- [ ] **3.2** Configurar banco de testes para integração:
  - Opção A: SQLite in-memory para testes rápidos (validar lógica, não SQL específico)
  - Opção B: Testcontainers + PostgreSQL para testes fiéis ao prod
  - Decisão documentada em Dev Notes; recomendar Testcontainers para Multi-Tenancy
- [ ] **3.3** Criar `DatabaseFixture` ou `IntegrationTestBase`:
  - Seed de 2 Advogados (A e B), clientes/processos para cada
  - Cleanup após cada teste (transação rollback ou Db recreado)
- [ ] **3.4** Escrever 1 teste de integração de exemplo:
  - Ex.: GET /api/health retorna 200 e status Healthy

### Phase 4: Multi-Tenancy Isolation Tests (~1.5h) **[CRÍTICO]**

- [ ] **4.1** Teste: _Query Filter aplicado – Advogado A não vê clientes do Advogado B_
  - Arrange: 2 advogados, 2 clientes (um por advogado)
  - Act: Setar tenant = Advogado A; listar clientes via repositório/API
  - Assert: Apenas 1 cliente retornado, e pertence ao Advogado A
- [ ] **4.2** Teste: _Query Filter aplicado – listagem de Processos isolada por tenant_
  - Arrange: Processos para Advogado A e B
  - Act: Listar processos como Advogado A
  - Assert: Nenhum processo do Advogado B na lista
- [ ] **4.3** Teste: _Insert automático de AdvogadoId – novo registro recebe tenant correto_
  - Act: Criar Cliente (sem setar AdvogadoId) com tenant = Advogado A
  - Assert: Cliente persistido com AdvogadoId = Advogado A
- [ ] **4.4** Teste: _Tentativa de acesso direto por ID (outro tenant) retorna 404 ou vazio_
  - Arrange: Cliente do Advogado B
  - Act: GET /api/clientes/{id} como Advogado A com ID do cliente de B
  - Assert: 404 Not Found (nunca 200 com dados do outro advogado)
- [ ] **4.5** Documentar cenários de “data leak” no Dev Notes para futuros testes de regressão

### Phase 5: CI Integration & Conventions (~0.5h)

- [ ] **5.1** Garantir que `dotnet test` rode todos os projetos de teste
- [ ] **5.2** Adicionar script ou target no pipeline: testes devem passar para build ser considerado OK
- [ ] **5.3** Documentar convenções no README ou docs/framework:
  - Nomenclatura: `MethodName_Scenario_ExpectedBehavior`
  - Unit tests: sem I/O, sem DB; mocks para tudo externo
  - Integration tests: podem usar DB de teste; limpar estado entre testes

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN a base de testes está configurada
WHEN o desenvolvedor executa `dotnet test`
THEN os projetos Agile360.UnitTests e Agile360.IntegrationTests são executados
AND todos os testes passam (verde)

GIVEN os testes de Multi-Tenancy estão implementados
WHEN os testes de isolamento são executados
THEN nenhum cenário retorna dados de um advogado diferente do tenant corrente
AND o teste "Insert automático de AdvogadoId" valida o TenantSaveChangesInterceptor

GIVEN dois advogados com dados no banco de testes
WHEN uma query é feita no contexto do Advogado A
THEN apenas entidades com AdvogadoId = A são retornadas
AND acessar por ID um recurso do Advogado B resulta em 404 (nunca em dados)

GIVEN o projeto de integração usa WebApplicationFactory
WHEN um teste de API é executado
THEN a aplicação sobe em memória
AND requisições autenticadas podem ser feitas com JWT de teste ou mock de ICurrentUserService
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Infraestrutura de testes é base para qualidade |
| Complexity | Medium | Multi-tenancy tests exigem cuidado com dados e contexto |
| Test Requirements | Unit + Integration | Cobrir isolamento e regras críticas |
| Review Focus | Correctness, Security | Garantir que testes realmente validam isolamento |

### Agent Assignment

| Role | Agent | Responsibility |
|------|-------|----------------|
| Primary | @dev | Implementação dos projetos e testes |
| Secondary | @qa | Validação dos cenários de isolamento e cobertura |
| Review | @architect | Garantir que cenários de data leak estão cobertos |

### Focus Areas

- [ ] Multi-Tenancy: Testes cobrem vazamento entre tenants
- [ ] Query Filters: Testes usam DbContext real (ou in-memory) com filters ativos
- [ ] Naming: Convenção clara para nome de testes
- [ ] No flaky tests: Integração com DB determinística (seed + cleanup)

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (API e projetos existentes)
- Story 1.2: Multi-Tenancy (Query Filters e interceptors para testar)

**Blocks:**
- Nenhuma story bloqueia; todas as demais devem adicionar testes conforme DoD

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Testes lentos (Testcontainers) | Medium | Usar SQLite in-memory para suite rápida; Testcontainers só para pipeline ou suite “heavy” |
| Program não acessível para WebApplicationFactory | Low | InternalsVisibleTo para test assembly |
| Flaky por estado compartilhado | High | Isolar estado por teste (transação rollback ou DB novo por test) |

---

## 📋 Definition of Done

- [x] Projetos Agile360.UnitTests e Agile360.IntegrationTests criados e na Solution
- [x] xUnit + NSubstitute + FluentAssertions configurados
- [x] WebApplicationFactory configurada para testes de API
- [x] Pelo menos 4 testes de isolamento Multi-Tenancy passando (4.1–4.4)
- [x] Pelo menos 2 testes unitários de exemplo (Application)
- [x] Pelo menos 1 teste de integração de API (ex.: health)
- [x] `dotnet test` executa e passa
- [x] Convenções documentadas (README ou docs/framework)
- [ ] All acceptance criteria verified

---

## 📝 Dev Notes

### Key Files

```
tests/
├── Agile360.UnitTests/
│   ├── Application/
│   │   └── Commands/
│   ├── Domain/
│   ├── Builders/
│   │   ├── AdvogadoBuilder.cs
│   │   ├── ClienteBuilder.cs
│   │   └── ProcessoBuilder.cs
│   ├── TestBase.cs
│   └── Agile360.UnitTests.csproj
│
├── Agile360.IntegrationTests/
│   ├── Api/
│   │   └── HealthCheckTests.cs
│   ├── MultiTenancy/
│   │   ├── TenantIsolationQueryFilterTests.cs
│   │   ├── TenantInsertInterceptorTests.cs
│   │   └── TenantAccessControlTests.cs
│   ├── Database/
│   │   └── DatabaseFixture.cs
│   ├── Agile360WebApplicationFactory.cs
│   └── Agile360.IntegrationTests.csproj
│
└── Directory.Build.props (optional)
```

### Database for Integration Tests

- **Recomendação:** Testcontainers + PostgreSQL para testes de Multi-Tenancy (RLS e SQL iguais ao prod).
- **Alternativa:** SQLite in-memory para velocidade; documentar que RLS não é testado nesse modo.
- Aplicar migrations no banco de teste antes dos testes.

### Testing Checklist

#### Unit
- [ ] Handlers testados com mocks (IRepository, IUnitOfWork)
- [ ] Validators testados com dados válidos e inválidos
- [ ] Result pattern testado (success/failure)

#### Integration
- [ ] Health check retorna 200
- [ ] Tenant A não vê dados de Tenant B (clientes, processos)
- [ ] Novo registro recebe AdvogadoId do contexto
- [ ] GET por ID de outro tenant retorna 404

---

## 🧑‍💻 Dev Agent Record

| Timestamp | Phase | Action | Result |
|-----------|-------|--------|--------|
| 2026-02-21 | 1–5 | Story 1.1.1 executada | UnitTests + IntegrationTests criados; 4 testes de isolamento multi-tenant; Repository.GetByIdAsync alterado para aplicar query filter (FirstOrDefaultAsync). docs/framework/testing-conventions.md criado. |

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story creation – Test Foundation | @architect (Aria) |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20  
**Contexto:** Base de testes vital para CRM jurídico; evitar vazamento de dados entre advogados.
