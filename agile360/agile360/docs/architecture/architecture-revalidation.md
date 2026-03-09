# Agile360 – Revalidação do Plano de Desenvolvimento e User Stories

**Versão:** 1.0.0  
**Status:** ✅ Aprovado com recomendações  
**Criado por:** @architect (Revalidação)  
**Data:** 2026-02-21  

Este documento revalida o plano de desenvolvimento, as User Stories da Epic 1 e a documentação técnica para garantir que o Agile360 seja construído seguindo **padrões de engenharia de software de alta performance**: interface simples para o advogado e arquitetura robusta e escalável por baixo.

---

## 1. Princípios de Engenharia e SOLID

### 1.1 Single Responsibility (SRP)

| Onde validar | Status | Evidência nas stories/docs |
|--------------|--------|----------------------------|
| **Services** | ✅ Alinhado | Story 1.1: Application layer com Commands/Queries; 1.3: `IAuthService` vs `ICurrentUserService`; 1.4: `IAiGatewayService` separado de webhook auth. Cada serviço tem uma razão única de mudança. |
| **Controllers** | ✅ Alinhado | Controllers finos: HealthCheck, Auth, Webhooks; responsabilidade limitada a HTTP e delegação para MediatR (1.1, 1.3, 1.4). |
| **Repositories** | ✅ Alinhado | Story 1.2: `IRepository<T>` base + repositórios especializados por entidade (`IClienteRepository`, `IProcessoRepository`, etc.) com métodos específicos de domínio; responsabilidade única de acesso a dados por agregado. |

**Recomendação:** Manter e documentar explicitamente na **Story 1.1** (Dev Notes) que *Controllers não contêm lógica de negócio; Services/Handlers contêm uma única responsabilidade por use case*.

---

### 1.2 Injeção de Dependência (DI)

| Onde validar | Status | Evidência nas stories/docs |
|--------------|--------|----------------------------|
| **Desacoplamento por interfaces** | ✅ Alinhado | Domain define `IRepository<T>`, `IUnitOfWork`, `ITenantProvider` (1.1, 1.2); Application define `IAuthService`, `ICurrentUserService`, `IAiGatewayService` (1.3, 1.4). Implementações na Infrastructure. |
| **Substituição de provedores** | ✅ Alinhado | Gaps-and-decisions: "IAiGatewayService – implementação pode ser n8n ou AIOS"; Story 1.4: registrar em DI `IAiGatewayService` → `N8nAiGatewayService` (ou stub). Provedor de e-mail e banco não estão no core; EF Core e Supabase são injetados via DbContext e connection string. |

**Recomendação:** Incluir na **system-architecture.md** uma subseção **"Princípio de substituição"**: *Nenhum use case da Application layer deve instanciar concretamente provedores de e-mail, banco ou IA; todas as dependências devem ser injetadas via construtor e registradas no container (API ou Infrastructure).*

---

### 1.3 Interface Segregation (ISP)

| Onde validar | Status | Evidência nas stories/docs |
|--------------|--------|----------------------------|
| **Interfaces por módulo** | ✅ Alinhado | Repositórios especializados (1.2): `IClienteRepository` expõe apenas `GetByCpfAsync`, `GetByWhatsAppAsync`, `SearchAsync`; `IPrazoRepository` apenas métodos de prazos. Nenhuma interface "gorda" única. |
| **Application** | ✅ Alinhado | Handlers MediatR por Command/Query; `IAuthService` vs `ICurrentUserService`; `IAiGatewayService` focado em extração. Cada consumidor depende só do que usa. |

**Recomendação:** Na **Story 1.2** (Dev Notes), acrescentar: *Ao criar novos repositórios ou serviços, preferir interfaces específicas por agregado/use case; não estender uma interface genérica com dezenas de métodos não relacionados.*

---

## 2. Camada de Infraestrutura e Persistência (EF Core)

### 2.1 DbContext & Migrations

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Schema gerenciado via Migrations** | ✅ Alinhado | Story 1.1: Agile360DbContext em Infrastructure/Data; 1.2: Phase 2 "Criar e executar a migration inicial: InitialSchema"; Fluent API por entidade. |
| **Versionamento do banco** | ✅ Alinhado | Migrations no código (1.2: `YYYYMMDD_InitialSchema.cs`); aplicação no Supabase documentada. |

**Recomendação:** Garantir na **Story 1.2** DoD que *todas as alterações de schema passem por novas Migrations (nunca alterar banco manualmente em produção sem migration correspondente)*.

---

### 2.2 Repositórios e isolamento do domínio

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Lógica de acesso a dados na Infrastructure** | ✅ Alinhado | Story 1.1: `Repository<T>` em Infrastructure/Repositories; 1.2: ClienteRepository, ProcessoRepository, etc. em Infrastructure. Domain contém apenas entidades e interfaces. |
| **Domínio protegido de detalhes do banco** | ✅ Alinhado | Domain não referencia EF Core; entidades são POCOs; configurações Fluent e interceptors ficam em Infrastructure. |

Nenhuma alteração necessária.

---

### 2.3 Multi-tenancy (isolamento por tenant)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Filtro global ou lógica centralizada** | ✅ Alinhado | system-architecture: "EF Core Global Query Filter" + "Tripla Proteção" (JWT → Query Filter → RLS). Story 1.2: Phase 3 – Global Query Filter no DbContext para Cliente, Processo, Audiência, Prazo, Nota, EntradaIA; TenantSaveChangesInterceptor. |
| **Advogado nunca acessa dados de outro** | ✅ Alinhado | Story 1.2: RLS no PostgreSQL; 1.1.1: testes explícitos 4.1–4.4 (Query Filter, insert automático, 404 ao acessar ID de outro tenant). |

**Recomendação:** Manter os testes de isolamento (1.1.1) como **gate obrigatório** no CI (Story 1.5); documentar em **system-architecture** que *qualquer nova entidade tenant-aware deve receber Query Filter e estar coberta por pelo menos um teste de isolamento*.

---

## 3. Modelagem de Classes e Camada de DTO

### 3.1 Classes de domínio

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Refletir realidade jurídica** | ✅ Alinhado | Story 1.2: entidades Cliente, Processo, Audiência, Prazo, Nota, EntradaIA com campos e enums de domínio (StatusProcesso, TipoAudiencia, etc.). |
| **Sem dependência de bibliotecas externas** | ✅ Alinhado | Story 1.1: "Domain deve ser livre de dependências externas"; NuGet em Domain: nenhum. |

Nenhuma alteração necessária.

---

### 3.2 DTO Layer (obrigatório)

| Requisito | Status | Ação |
|-----------|--------|------|
| **Separação entidade ↔ API/Frontend** | ⚠️ Implícito | Stories citam DTOs (1.1 Application: DTOs; 1.3: RegisterRequest, AuthResponse, AdvogadoProfileResponse). Falta **regra explícita** de que nenhuma entidade EF Core é exposta na API. |

**Recomendação (aplicada):**  
- Incluir na **system-architecture.md** a regra: *"Nenhuma entidade do EF Core (Domain/Infrastructure) deve ser serializada diretamente na API ou retornada ao front-end; usar sempre DTOs da Application layer."*  
- Na **Story 1.1** (Phase 2 ou Dev Notes): tarefa ou convenção: *"DTOs em Application: request/response específicos por endpoint; mapeamento Entity → DTO nos Handlers ou em mappers dedicados (ex.: não retornar `Cliente` e sim `ClienteResponse`)."*  
- Na **Story 1.2** ou Epic 2: reforçar que CRUD de Clientes/Processos usa DTOs (ClienteResponse, ProcessoResponse, etc.), nunca entidades.

---

### 3.3 Validação de entrada

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Validação antes da persistência** | ✅ Alinhado | Story 1.1: FluentValidation + ValidationBehavior do MediatR; 1.3: RegisterRequestValidator, LoginRequestValidator. Dados validados antes de chegarem aos Handlers e ao banco. |

**Recomendação:** Documentar em **system-architecture** ou **gaps-and-decisions** que *todo Command/Request que altera estado deve ter um Validator registrado; falhas de validação retornam 400 com mensagens claras, sem atingir a camada de persistência*.

---

## 4. Visão de Produto: Simplicidade para o Advogado

### 4.1 Dashboard como hub central

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **"Próximos Passos" claros** | ✅ Planejado | Epic 6 (Dashboard Hub Central); system-architecture: "AI Daily Briefing", "Próximos Passos". Stories 6.1–6.5 cobrem briefing, inbox de IA, audiências/prazos, calendário, processos recentes. |
| **Prazos, compromissos e audiências assíncronos** | ✅ Planejado | Fluxos 3 e 4 (Guardião de Prazos, Daily Briefing) e endpoints GET assíncronos; Story 1.2: repositórios com GetHojeAsync, GetVencimentoProximoAsync. |

**Recomendação:** Na **Epic 6** ou em story 6.1, deixar explícito: *Dashboard deve carregar dados em paralelo (audiências hoje, prazos vencendo, briefing) para garantir tempo de resposta baixo; evitar cascata de chamadas síncronas no front-end.*

---

### 4.2 Abstração total (complexidade invisível)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **CRUD intuitivo de clientes e processos** | ✅ Planejado | Epic 2: CRUD Clientes (2.1), Processos (2.2), Audiências (2.3); vinculação (2.4). |
| **EF Core e n8n invisíveis para o advogado** | ✅ Alinhado | Backend expõe apenas API REST; frontend consome DTOs e fluxos de negócio; integrações (n8n, Evolution API) são server-side. |

Nenhuma alteração necessária.

---

### 4.3 Gerenciamento total e feedback imediato

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| **Consultar, cadastrar, excluir, alterar de forma fluida** | ✅ Planejado | Epic 2 CRUD; API com padrão REST; cliente TypeScript (1.1 Task 2.6) para frontend tipado. |
| **Feedback imediato na tela** | ⚠️ Frontend | Depende da implementação do Epic 7; recomendar na documentação do Dashboard/Epic 7: uso de loading states, toasts/feedback visual em toda ação (create/update/delete). |

**Recomendação:** Incluir no **PRD Epic 1** ou em **Epic 7** um princípio de UX: *Toda ação do usuário (salvar, excluir, enviar) deve ter feedback visual imediato (loading + sucesso/erro); nunca deixar o advogado sem confirmação.*

---

## 5. Resumo de Ações e Checklist

### 5.1 Documentação a atualizar

| Documento | Alteração |
|-----------|-----------|
| **system-architecture.md** | Adicionar: (1) princípio de substituição (DI); (2) regra de não expor entidades na API (sempre DTOs); (3) validação obrigatória antes da persistência; (4) nova entidade tenant-aware → Query Filter + teste de isolamento. |
| **gaps-and-decisions.md** | Opcional: registrar "DTO obrigatório na API" e "Validator por Command que altera estado" na tabela de verificação. |
| **Story 1.1** | Dev Notes: convenção SRP (Controllers/Services); convenção DTO + mapeamento Entity→DTO. |
| **Story 1.2** | DoD: alterações de schema apenas via Migrations. Dev Notes: ISP para novos repositórios. |
| **Epic 6 / Story 6.1** | Carregamento assíncrono/paralelo do Dashboard; "Próximos Passos" como foco. |
| **Epic 7 ou PRD** | Princípio de feedback imediato em toda ação do usuário. |

### 5.2 Checklist de conformidade (desenvolvedor)

- [ ] Nenhuma entidade EF Core retornada diretamente na API; sempre DTOs.
- [ ] Todo Command que altera estado possui FluentValidator registrado.
- [ ] Novas entidades tenant-aware: Global Query Filter + RLS + teste de isolamento (1.1.1).
- [ ] Novas dependências externas (e-mail, IA, storage): injetadas via interface; implementação na Infrastructure.
- [ ] Controllers finos; lógica em Handlers/Services.
- [ ] Interfaces específicas por agregado/use case (evitar interfaces "gordas").

---

## 6. Conclusão

O **plano de desenvolvimento e as User Stories da Epic 1** estão **alinhados** com:

- **SOLID:** SRP, DI e ISP contemplados nas stories e na estrutura Clean Architecture; reforços sugeridos são documentais e de convenção.
- **Infraestrutura e persistência:** DbContext e Migrations como única fonte de verdade do schema; repositórios na Infrastructure; multi-tenancy com filtro global, interceptor e RLS, e testes de isolamento.
- **Domínio e DTO:** Domínio puro; validação com FluentValidation. A **separação obrigatória DTO vs entidade** foi reforçada na documentação e nas recomendações.
- **Visão de produto:** Dashboard como hub, CRUD completo planejado, abstração da complexidade técnica; recomendações para carregamento assíncrono e feedback imediato na UX.

Com as **ações recomendadas** (atualizações em system-architecture, Story 1.1, 1.2, Epic 6/7 e checklist), o Agile360 fica formalmente revalidado para ser construído como **facilitador extremo para o advogado** com **arquitetura robusta e escalável**.

---

**Criado por:** @architect  
**Data:** 2026-02-21
