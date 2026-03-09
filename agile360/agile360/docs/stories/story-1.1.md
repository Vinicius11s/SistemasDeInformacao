# Story 1.1: Project Scaffolding – .NET 9 API + Supabase Setup

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.1
**Sprint:** 1
**Priority:** 🔴 Critical
**Points:** 8
**Effort:** 6-10 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| .NET 9 como backend | PRD Agile360 | Define o scaffolding como Web API .NET 9 |
| Supabase como DB | PRD Agile360 | Conexão PostgreSQL via Supabase connection string |
| Multi-Tenancy por advogado | PRD Agile360 | Schema deve prever `advogado_id` desde o início |
| Dark Theme + Cores do logo | PRD Agile360 | Não impacta backend diretamente |
| Base de testes vital (CRM jurídico) | Requisito | Story 1.1.1 cobre Unit + Integration (xUnit/NSubstitute) e testes de isolamento tenant |
| Cliente TypeScript para Frontend | Epic 7 / API Docs | Task 2.6 nesta story: gerar cliente TypeScript a partir do OpenAPI (Swagger) |

---

## 📋 User Story

**Como** desenvolvedor do Agile360,
**Quero** ter a estrutura base do projeto .NET 9 Web API configurada com conexão ao Supabase,
**Para** poder iniciar o desenvolvimento das funcionalidades do CRM jurídico com uma base sólida e padronizada.

---

## 🎯 Objective

Criar o scaffolding completo do projeto Agile360 com .NET 9 Web API, configurar a conexão com o Supabase (PostgreSQL), estruturar a solução em camadas (Clean Architecture) e estabelecer os padrões de projeto que serão seguidos em todo o desenvolvimento. Esta story é a fundação sobre a qual todo o sistema será construído.

---

## ✅ Tasks

### Phase 1: Solution Structure (.NET 9) (~2h)

- [ ] **1.1** Criar a Solution `.sln` com o nome `Agile360`
- [ ] **1.2** Criar os projetos seguindo Clean Architecture:
  - `Agile360.API` – Web API (Controllers, Middleware, Configuration)
  - `Agile360.Application` – Use Cases, DTOs, Interfaces, Validators
  - `Agile360.Domain` – Entities, Value Objects, Enums, Domain Events
  - `Agile360.Infrastructure` – Data Access, External Services, Repositories
  - `Agile360.Shared` – Cross-cutting concerns (Extensions, Helpers, Constants)
- [ ] **1.3** Configurar referências entre projetos (dependency flow):
  ```
  API → Application → Domain
  API → Infrastructure → Application → Domain
  API → Shared
  ```
- [ ] **1.4** Configurar `global.json` com .NET 9 SDK version
- [ ] **1.5** Criar `.editorconfig` com padrões C# (naming, formatting, analyzers)

### Phase 2: API Base Configuration (~2h)

- [ ] **2.1** Configurar `Program.cs` com:
  - Dependency Injection container
  - CORS policy (permitir frontend)
  - Swagger/OpenAPI documentation
  - Global exception handler middleware
  - Health check endpoints
  - JSON serialization (camelCase, DateTimeOffset)
- [ ] **2.2** Criar middleware de tratamento global de exceções:
  - `ExceptionHandlingMiddleware.cs`
  - Retorno padronizado: `{ success, data, error, timestamp }`
- [ ] **2.3** Configurar `appsettings.json` e `appsettings.Development.json`:
  - Connection strings (Supabase)
  - JWT settings
  - CORS origins
  - Logging levels
- [ ] **2.4** Criar `ApiResponse<T>` (response wrapper padrão)
- [ ] **2.5** Criar `HealthCheckController` com endpoint `/api/health`
- [ ] **2.6** Configurar geração de **Cliente TypeScript** a partir do OpenAPI (Swagger):
  - Usar `NSwag` ou `OpenApi.Generator` para gerar cliente tipado em build ou via script
  - Output sugerido: `frontend/src/api/generated/` (ou doc no README para frontend consumir)
  - Objetivo: Frontend (Epic 7) consumir API com tipos alinhados ao contrato; detalhes em [Gaps e Decisões](docs/architecture/gaps-and-decisions.md)

### Phase 3: Supabase Database Connection (~2h)

- [ ] **3.1** Instalar pacotes NuGet:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core provider)
  - `Microsoft.EntityFrameworkCore.Design` (migrations)
  - `Microsoft.EntityFrameworkCore.Tools`
- [ ] **3.2** Criar `Agile360DbContext` em `Infrastructure/Data/`:
  - Configuração de connection string via `IConfiguration`
  - Override de `OnModelCreating` para configurações fluent
- [ ] **3.3** Configurar connection string do Supabase em `appsettings`:
  ```json
  {
    "ConnectionStrings": {
      "Supabase": "Host=<project>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<password>;SSL Mode=Require;Trust Server Certificate=true"
    }
  }
  ```
- [ ] **3.4** Criar `IUnitOfWork` interface e implementação
- [ ] **3.5** Criar `BaseEntity` com campos padrão:
  - `Id` (Guid), `CreatedAt`, `UpdatedAt`, `IsActive`, `AdvogadoId`
- [ ] **3.6** Testar conexão com Supabase via health check

### Phase 4: Padrões e Infraestrutura de Código (~2h)

- [ ] **4.1** Configurar logging estruturado com `Serilog`:
  - Console sink (desenvolvimento)
  - Structured JSON format
  - Correlation ID per request
- [ ] **4.2** Criar `Result<T>` pattern para retorno de operações:
  ```csharp
  public class Result<T> {
      public bool IsSuccess { get; }
      public T? Value { get; }
      public string? Error { get; }
  }
  ```
- [ ] **4.3** Configurar `FluentValidation` para validação de requests
- [ ] **4.4** Criar `IRepository<T>` interface genérica (Repository Pattern)
- [ ] **4.5** Criar `Repository<T>` implementação base com EF Core
- [ ] **4.6** Configurar `MediatR` para CQRS pattern (Commands & Queries)
- [ ] **4.7** Criar pipeline behaviors do MediatR:
  - `ValidationBehavior` (FluentValidation automático)
  - `LoggingBehavior` (log de todas as operações)
- [ ] **4.8** Criar `.env.example` com variáveis necessárias
- [ ] **4.9** Atualizar `.gitignore` para .NET projects

### Phase 5: Docker & DevOps Básico (~1h)

- [ ] **5.1** Criar `Dockerfile` multi-stage para a API:
  - Build stage (SDK)
  - Runtime stage (ASP.NET Runtime)
  - Health check
- [ ] **5.2** Criar `docker-compose.yml` com:
  - API service
  - Variáveis de ambiente
  - Network configuration
- [ ] **5.3** Criar `README.md` do projeto com:
  - Descrição do Agile360
  - Stack tecnológica
  - Setup local (pré-requisitos, comandos)
  - Estrutura de pastas
  - Padrões de código

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN o projeto Agile360 scaffolding está completo
WHEN o desenvolvedor executa `dotnet build`
THEN a solução compila sem erros em todos os projetos

GIVEN a API está rodando
WHEN uma requisição GET é feita para `/api/health`
THEN retorna 200 OK com status "Healthy" e versão da API

GIVEN o Supabase está configurado
WHEN a API inicializa
THEN a conexão com o PostgreSQL é estabelecida com sucesso
AND o health check de database retorna "Healthy"

GIVEN o middleware de exceções está ativo
WHEN uma exceção não tratada ocorre em qualquer endpoint
THEN retorna JSON padronizado com `{ success: false, error: {...}, timestamp }`
AND a exceção é logada no Serilog com correlation ID

GIVEN o Swagger está configurado
WHEN o desenvolvedor acessa `/swagger`
THEN a documentação interativa da API é exibida
AND todos os endpoints estão documentados

GIVEN o Docker está configurado
WHEN `docker compose up` é executado
THEN a API sobe e responde no porta configurada
AND o health check retorna sucesso
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Scaffolding e configuração base do projeto |
| Complexity | Medium | Múltiplas camadas, mas padrões bem definidos |
| Test Requirements | Integration | Testar conexão DB, health checks, middleware |
| Review Focus | Architecture, Security | Garantir padrões corretos desde o início |

### Agent Assignment

| Role | Agent | Responsibility |
|------|-------|----------------|
| Primary | @dev | Implementação do scaffolding |
| Secondary | @architect | Validação da arquitetura Clean Architecture |
| Review | @qa | Testes de integração e health checks |

### Self-Healing Config

```yaml
reviews:
  auto_review:
    enabled: true
    drafts: false
  path_instructions:
    - path: "src/Agile360.API/**"
      instructions: "Verificar configuração de DI, middleware pipeline, CORS"
    - path: "src/Agile360.Domain/**"
      instructions: "Garantir que entities seguem DDD, sem dependências externas"
    - path: "src/Agile360.Infrastructure/**"
      instructions: "Verificar EF Core config, connection string segura, repository pattern"

chat:
  auto_reply: true
```

### Focus Areas

- [ ] Clean Architecture: Dependency flow correto (Domain sem dependências externas)
- [ ] Connection String: Supabase configurada de forma segura (sem hardcode)
- [ ] Exception Handling: Middleware global captura todas as exceções
- [ ] CQRS: MediatR configurado com pipeline behaviors
- [ ] Health Checks: Database e API respondendo corretamente

---

## 🔗 Dependencies

**Blocked by:**
- Nenhuma (primeira story do projeto)

**Blocks:**
- Story 1.1.1: Test Foundation (projetos de teste referenciam API/Application)
- Story 1.2: Multi-Tenancy Architecture & Database Schema
- Story 1.3: Authentication & Authorization
- Story 1.4: Integration Foundation (API base para webhooks)
- Story 1.5: CI/CD Pipeline (build e test)
- Todas as stories subsequentes (Epic 2-7)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Incompatibilidade EF Core + Supabase | High | Testar conexão logo na Phase 3, usar Npgsql provider testado |
| .NET 9 SDK não instalado | Low | Documentar versão exata no global.json e README |
| Supabase connection timeout | Medium | Configurar retry policy e connection pooling |
| Over-engineering na fundação | Medium | Manter YAGNI – só o necessário para as próximas stories |

---

## 📋 Definition of Done

- [ ] Solução .NET 9 compila sem erros
- [ ] 5 projetos criados seguindo Clean Architecture
- [ ] Conexão com Supabase PostgreSQL funcionando
- [ ] Health check endpoint `/api/health` retornando 200
- [ ] Swagger acessível em `/swagger`
- [ ] Middleware de exceções tratando erros globalmente
- [ ] Serilog configurado com logging estruturado
- [ ] MediatR + FluentValidation configurados
- [ ] Repository pattern implementado
- [ ] Dockerfile e docker-compose funcionando
- [ ] README.md completo
- [ ] `.gitignore` e `.editorconfig` configurados
- [ ] All acceptance criteria verified
- [ ] Tests passing
- [ ] Documentation updated

---

## 📝 Dev Notes

### Key Files

```
Agile360/
├── src/
│   ├── Agile360.API/
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   │   └── HealthCheckController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Models/
│   │   │   └── ApiResponse.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── Agile360.Application/
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── LoggingBehavior.cs
│   │   ├── Common/
│   │   │   └── Result.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Agile360.Domain/
│   │   ├── Entities/
│   │   │   └── BaseEntity.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Enums/
│   │
│   ├── Agile360.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Agile360DbContext.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Repositories/
│   │   │   └── Repository.cs
│   │   └── DependencyInjection.cs
│   │
│   └── Agile360.Shared/
│       ├── Extensions/
│       ├── Helpers/
│       └── Constants/
│
├── tests/
│   └── Agile360.IntegrationTests/
│
├── Agile360.sln
├── global.json
├── .editorconfig
├── .gitignore
├── .env.example
├── Dockerfile
├── docker-compose.yml
└── README.md
```

### Convenções de arquitetura (SOLID / DTO)

- **SRP:** Controllers não contêm lógica de negócio; Services/Handlers têm uma única responsabilidade por use case.
- **DTO:** Usar sempre DTOs em Application (request/response por endpoint). Mapeamento Entity → DTO nos Handlers ou em mappers dedicados; nunca retornar entidades do EF Core na API (ex.: retornar `ClienteResponse`, não `Cliente`).

### Technical Notes

**NuGet Packages (API):**
- `Swashbuckle.AspNetCore` – Swagger UI
- `Serilog.AspNetCore` – Structured logging
- `Serilog.Sinks.Console` – Console output

**NuGet Packages (Application):**
- `MediatR` – CQRS mediator
- `FluentValidation` – Request validation
- `FluentValidation.DependencyInjectionExtensions`

**NuGet Packages (Infrastructure):**
- `Npgsql.EntityFrameworkCore.PostgreSQL` – PostgreSQL EF Core
- `Microsoft.EntityFrameworkCore.Design` – EF migrations

**NuGet Packages (Domain):**
- Nenhum (Domain deve ser livre de dependências externas)

### Testing Checklist

#### Build & Compilation
- [ ] `dotnet build` compila sem warnings
- [ ] `dotnet run --project src/Agile360.API` inicia sem erros

#### Health Checks
- [ ] `GET /api/health` retorna 200
- [ ] Database health check retorna "Healthy" com Supabase conectado
- [ ] Database health check retorna "Unhealthy" sem conexão

#### Middleware
- [ ] Exceção não tratada retorna JSON padronizado
- [ ] Correlation ID presente nos logs
- [ ] Status codes corretos (400, 404, 500)

#### Docker
- [ ] `docker compose up --build` funciona
- [ ] Container responde na porta configurada

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
