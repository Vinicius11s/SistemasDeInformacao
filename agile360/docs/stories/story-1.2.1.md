# Story 1.2.1: Audit Trail Foundation – Shadow Properties & Audit Log

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.2.1
**Sprint:** 1
**Priority:** 🟠 High
**Points:** 5
**Effort:** 4-6 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| Auditoria sistêmica em sistemas jurídicos | Requisito jurídico | Toda alteração em Processo/Prazo (e entidades críticas) deve ser rastreada |
| Story 1.3 cita Audit Log para autenticação | Story 1.3 | Estender para auditoria de dados: quem criou/alterou cada registro |
| EF Core como ORM | Story 1.1 | Usar Shadow Properties e SaveChanges interceptor para preencher automaticamente |

---

## 📋 User Story

**Como** advogado ou auditor do Agile360,
**Quero** que todas as alterações em Processos, Prazos e entidades críticas registrem quem criou e quem alterou,
**Para** cumprir exigências de rastreabilidade e responsabilidade em um CRM jurídico.

---

## 🎯 Objective

Implementar a base de **Audit Trail** no Agile360: (1) Shadow Properties no EF Core (`CreatedBy`, `LastModifiedBy`, opcionalmente `CreatedAt`/`UpdatedAt` já existentes em BaseEntity) em entidades auditáveis; (2) interceptor que preenche automaticamente esses campos a partir do usuário corrente (AdvogadoId); (3) opcionalmente tabela de log de auditoria para histórico de alterações (old/new values) em entidades sensíveis. Foco em Processo e Prazo; extensível para Cliente, Audiência e outras.

---

## ✅ Tasks

### Phase 1: Shadow Properties & Base Auditable (~1.5h)

- [ ] **1.1** Definir interface `IAuditableEntity` (opcional) ou convenção: entidades que possuem `CreatedBy` e `LastModifiedBy` (Guids, nullable para CreatedBy na criação).
- [ ] **1.2** Em todas as entidades auditáveis (Processo, Prazo, Cliente, Audiencia, Nota), configurar Shadow Properties no EF Core:
  - `CreatedBy` (Guid?, FK conceitual para Advogado)
  - `LastModifiedBy` (Guid?, FK conceitual para Advogado)
  - Manter `CreatedAt` e `UpdatedAt` já existentes em BaseEntity (ou migrar para shadow se preferir consistência).
- [ ] **1.3** Configurar no Fluent API (por entidade ou convenção):
  - `Property<Guid?>("CreatedBy")`
  - `Property<Guid?>("LastModifiedBy")`
  - Não expor como propriedade na entity (shadow); apenas no banco e em queries de auditoria.
- [ ] **1.4** Criar migration `AddAuditShadowProperties`: adicionar colunas `created_by`, `last_modified_by` nas tabelas definidas (processos, prazos, clientes, audiencias, notas).

### Phase 2: SaveChanges Interceptor para Audit (~1.5h)

- [ ] **2.1** Criar `AuditSaveChangesInterceptor` (implementar `ISaveChangesInterceptor` ou `IInterceptor` do EF Core):
  - Em `SavingChanges`: percorrer entradas `Added` e `Modified` que são auditáveis.
  - Para `Added`: setar `CreatedBy` e `LastModifiedBy` com o AdvogadoId corrente (do `ITenantProvider` ou `ICurrentUserService`).
  - Para `Modified`: setar apenas `LastModifiedBy` (e `UpdatedAt` se não for automático).
  - Usar `context.UpdateEntry(entity).Property("LastModifiedBy").CurrentValue = currentUserId`.
- [ ] **2.2** Registrar o interceptor no `Agile360DbContext` (AddInterceptors).
- [ ] **2.3** Garantir que em contexto de background (ex.: webhook sem usuário) o interceptor não quebre: usar valor null para CreatedBy/LastModifiedBy ou um “system” user; documentar comportamento.
- [ ] **2.4** Teste unitário ou de integração: ao criar/atualizar um Processo com usuário logado, verificar que `created_by` e `last_modified_by` estão preenchidos no banco.

### Phase 3: Tabela de Auditoria (Opcional – Histórico) (~1.5h)

- [ ] **3.1** Decidir escopo: apenas shadow properties nesta story, ou também tabela de log.
  - **Recomendação:** Implementar tabela `audit_logs` para entidades críticas (Processo, Prazo) para rastrear “quem mudou o quê e quando”.
- [ ] **3.2** Criar entidade/tabela `AuditLog`:
  - Id (Guid), EntityName (string), EntityId (Guid), Action (Created/Updated/Deleted), AdvogadoId (Guid?), OldValues (jsonb, nullable), NewValues (jsonb, nullable), ChangedAt (DateTimeUtc), IpAddress (string, nullable).
- [ ] **3.3** No mesmo interceptor (ou outro), para entidades configuradas, gravar linha em `AuditLog` em `SavingChanges` (para Modified: old values podem ser obtidos via `context.UpdateEntry(entity).OriginalValues`).
- [ ] **3.4** Criar migration para `audit_logs`; aplicar.
- [ ] **3.5** (Opcional) Endpoint `GET /api/audit-logs?entity=Processo&entityId=...` para consulta por advogado (somente seu tenant); implementar em story futura se necessário.

### Phase 4: Documentação e RLS (~0.5h)

- [ ] **4.1** Documentar em Architecture: quais entidades são auditáveis; significado de CreatedBy/LastModifiedBy; uso da tabela audit_logs.
- [ ] **4.2** Aplicar RLS na tabela `audit_logs`: apenas o advogado dono do tenant pode ler seus próprios logs.
- [ ] **4.3** Atualizar Story 1.2 (ou doc de schema) com referência a shadow properties e audit_logs.

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN um Processo ou Prazo é criado por um advogado logado
WHEN SaveChanges é chamado
THEN as colunas created_by e last_modified_by são preenchidas com o AdvogadoId do usuário corrente

GIVEN um Processo ou Prazo é atualizado por um advogado logado
WHEN SaveChanges é chamado
THEN last_modified_by é atualizado com o AdvogadoId do usuário corrente
AND updated_at é atualizado

GIVEN a tabela audit_logs existe e o interceptor está ativo
WHEN uma entidade auditável (ex.: Processo) é alterada
THEN uma linha é inserida em audit_logs com entity_name, entity_id, action, advogado_id, new_values (e old_values para update)
AND apenas o advogado dono pode consultar seus logs (RLS)

GIVEN um contexto sem usuário (ex.: webhook)
WHEN uma entidade auditável é salva
THEN created_by/last_modified_by podem ser null (ou "system")
AND não há exceção
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Auditoria é cross-cutting e infra |
| Complexity | Medium | Interceptor + shadow properties + opcional audit_logs |
| Test Requirements | Unit + Integration | Garantir preenchimento correto e isolamento por tenant |
| Review Focus | Correctness, Security | Dados de auditoria não vazarem entre tenants |

### Focus Areas

- [ ] Shadow properties aplicadas de forma consistente em todas as entidades auditáveis
- [ ] Interceptor usa apenas usuário do tenant corrente (ICurrentUserService/ITenantProvider)
- [ ] audit_logs com RLS para não expor logs de outros advogados

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (DbContext, BaseEntity)
- Story 1.2: Multi-Tenancy (entidades Processo, Prazo, etc.; ITenantProvider)
- Story 1.3: Authentication (ICurrentUserService / AdvogadoId do JWT)

**Blocks:**
- Nenhuma story bloqueia; auditoria é requisito para produção em contexto jurídico

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance ao gravar audit_logs em todo update | Medium | Inserção assíncrona ou em lote; considerar apenas entidades críticas |
| OldValues muito grandes (jsonb) | Low | Limitar tamanho ou armazenar apenas campos alterados |
| Webhook/sistema sem usuário | Low | Permitir null em CreatedBy/LastModifiedBy; documentar |

---

## 📋 Definition of Done

- [ ] Shadow properties CreatedBy e LastModifiedBy configuradas nas entidades auditáveis (Processo, Prazo, Cliente, Audiencia, Nota)
- [ ] Migration aplicada (colunas created_by, last_modified_by)
- [ ] AuditSaveChangesInterceptor implementado e registrado
- [ ] Teste(s) validando preenchimento de created_by e last_modified_by
- [ ] (Opcional) Tabela audit_logs criada e preenchida no interceptor; RLS aplicada
- [ ] Documentação atualizada
- [ ] All acceptance criteria verified

---

## 📝 Dev Notes

### Key Files

```
src/Agile360.Infrastructure/
├── Data/
│   ├── Interceptors/
│   │   └── AuditSaveChangesInterceptor.cs
│   ├── Configurations/
│   │   └── (atualizar Processo, Prazo, Cliente, Audiencia, Nota com shadow properties)
│   └── Migrations/
│       └── YYYYMMDD_AddAuditShadowProperties.cs
│       └── YYYYMMDD_AddAuditLogTable.cs (se Phase 3)
```

### Entidades auditáveis (sugestão)

- Processo, Prazo (críticas)
- Cliente, Audiencia, Nota (recomendado)
- Advogado: apenas se houver fluxo de alteração por outro ator; senão opcional

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story – Audit Trail Foundation | @architect (Aria) |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20
