# Agile360 – System Architecture

**Versão:** 1.0.0
**Status:** ⚪ Draft
**Criado por:** Aria (@architect)
**Data:** 2026-02-20

---

## 📋 Visão Geral da Arquitetura

O Agile360 segue uma arquitetura **Clean Architecture** no backend (.NET 9), com **Multi-Tenancy** por advogado, integração com **Supabase** (PostgreSQL + Auth), orquestração de fluxos via **n8n** e inteligência artificial via **GPT-4o**.

---

## 🏗️ Diagrama de Arquitetura de Alto Nível

```
┌────────────────────────────────────────────────────────────────────────────┐
│                           AGILE360 ARCHITECTURE                           │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────────────────┐   │
│  │   Frontend    │     │   WhatsApp   │     │      Email (IMAP)       │   │
│  │  (Dashboard)  │     │ (Evolution   │     │   (Intimações)          │   │
│  │  Desktop-1st  │     │    API)      │     │                         │   │
│  └──────┬───────┘     └──────┬───────┘     └──────────┬──────────────┘   │
│         │                    │                         │                   │
│         │ HTTPS              │ Webhook                 │ IMAP Poll         │
│         ▼                    ▼                         ▼                   │
│  ┌──────────────────────────────────────────────────────────────────┐     │
│  │                        n8n (Orquestrador)                        │     │
│  │  ┌────────────┐  ┌──────────────┐  ┌──────────────────────┐     │     │
│  │  │ WhatsApp   │  │   Email      │  │   Guardião de        │     │     │
│  │  │ Flow       │  │   Flow       │  │   Prazos Flow        │     │     │
│  │  │            │  │              │  │                      │     │     │
│  │  │ Webhook →  │  │ IMAP Read → │  │ Cron → Check       │     │     │
│  │  │ GPT-4o →   │  │ GPT-4o →    │  │ Prazos → Alert     │     │     │
│  │  │ API Call   │  │ API Call     │  │ via WhatsApp        │     │     │
│  │  └────────────┘  └──────────────┘  └──────────────────────┘     │     │
│  └──────────────────────────┬──────────────────────────────────────┘     │
│                              │                                            │
│                              │ REST API Calls                             │
│                              ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────┐     │
│  │                    .NET 9 Web API (C#)                           │     │
│  │                    Clean Architecture                            │     │
│  │                                                                  │     │
│  │  ┌──────────┐  ┌──────────────┐  ┌──────────────────────────┐  │     │
│  │  │   API    │  │ Application  │  │        Domain            │  │     │
│  │  │  Layer   │  │    Layer     │  │        Layer             │  │     │
│  │  │          │  │              │  │                          │  │     │
│  │  │ Control- │→ │ Commands/    │→ │ Entities                │  │     │
│  │  │ lers     │  │ Queries      │  │ Value Objects           │  │     │
│  │  │ Middle-  │  │ Validators   │  │ Enums                   │  │     │
│  │  │ ware     │  │ DTOs         │  │ Interfaces              │  │     │
│  │  │ Filters  │  │ MediatR      │  │ Domain Events           │  │     │
│  │  └──────────┘  └──────────────┘  └──────────────────────────┘  │     │
│  │                                                                  │     │
│  │  ┌──────────────────────┐  ┌──────────────────────────────┐    │     │
│  │  │   Infrastructure     │  │          Shared              │    │     │
│  │  │                      │  │                              │    │     │
│  │  │ EF Core DbContext    │  │ Extensions                   │    │     │
│  │  │ Repositories         │  │ Helpers                      │    │     │
│  │  │ Supabase Auth Client │  │ Constants                    │    │     │
│  │  │ External Services    │  │ Cross-cutting                │    │     │
│  │  └──────────┬───────────┘  └──────────────────────────────┘    │     │
│  └─────────────┼────────────────────────────────────────────────────┘     │
│                │                                                          │
│                │ Npgsql + EF Core                                         │
│                ▼                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐     │
│  │                    Supabase (PostgreSQL)                         │     │
│  │                                                                  │     │
│  │  ┌──────────┐  ┌──────────────┐  ┌──────────────────────────┐  │     │
│  │  │  Auth    │  │  Database    │  │     Row Level Security   │  │     │
│  │  │          │  │              │  │                          │  │     │
│  │  │ Users    │  │ advogados    │  │ Tenant Isolation         │  │     │
│  │  │ Sessions │  │ clientes     │  │ Per-advogado policies    │  │     │
│  │  │ JWT      │  │ processos    │  │ Auto-filter queries      │  │     │
│  │  │          │  │ audiencias   │  │                          │  │     │
│  │  │          │  │ prazos       │  │                          │  │     │
│  │  │          │  │ notas        │  │                          │  │     │
│  │  │          │  │ entradas_ia  │  │                          │  │     │
│  │  └──────────┘  └──────────────┘  └──────────────────────────┘  │     │
│  └──────────────────────────────────────────────────────────────────┘     │
│                                                                            │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────────────────┐   │
│  │ Google       │     │   GPT-4o     │     │   Evolution API         │   │
│  │ Agenda API   │     │  (via n8n)   │     │   (WhatsApp)            │   │
│  │ (Sync)       │     │              │     │                         │   │
│  └──────────────┘     └──────────────┘     └──────────────────────────┘   │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxos de Dados Principais

### Fluxo 1: Captura Neural via WhatsApp

```
Cliente envia mensagem WhatsApp
        │
        ▼
Evolution API (recebe mensagem)
        │
        ▼ Webhook POST
n8n Workflow "WhatsApp Capture"
        │
        ├─── 1. Limpar dados (regex @s.whatsapp.net)
        ├─── 2. Classificar tipo (áudio/texto)
        ├─── 3. Se áudio → Whisper API → Transcrição
        ├─── 4. GPT-4o: Extrair (Nome, CPF, Detalhes do Caso)
        ├─── 5. Validar dados extraídos
        │
        ▼ POST /api/entradas-ia
.NET API → Cria EntradaIA com DadosExtraidos (JSON)
        │
        ├─── Se cliente já existe (CPF match) → Vincular
        └─── Se novo → Criar Cliente com status "Pendente Review"
```

### Fluxo 2: Monitor de Intimações

```
Email chega na caixa do advogado
        │
        ▼ IMAP Poll (a cada 5 min)
n8n Workflow "Email Monitor"
        │
        ├─── 1. Ler novos e-mails não lidos
        ├─── 2. Filtrar por remetentes relevantes (tribunais, TJ)
        ├─── 3. GPT-4o: Extrair dados da intimação
        │       - Número do processo
        │       - Tipo de intimação
        │       - Prazo (se houver)
        │       - Data de audiência (se houver)
        ├─── 4. Validar dados extraídos
        │
        ▼ POST /api/entradas-ia
.NET API → Cria EntradaIA (origem: Email)
        │
        ├─── Buscar processo por número
        ├─── Se tem prazo → Criar Prazo
        ├─── Se tem audiência → Criar Audiência
        └─── Notificar advogado via WhatsApp
```

### Fluxo 3: Guardião de Prazos

```
Cron Job (a cada 1 hora)
        │
        ▼
n8n Workflow "Deadline Guardian"
        │
        ├─── 1. GET /api/prazos/vencendo?horas=24
        │       (Busca prazos vencendo em 24h)
        │
        ├─── 2. Para cada prazo:
        │       ├── Classificar urgência
        │       ├── Montar mensagem de alerta
        │       └── Enviar via Evolution API (WhatsApp)
        │
        ├─── 3. GET /api/prazos/fatais?status=pendente
        │       (Busca prazos fatais ainda pendentes)
        │
        └─── 4. Alertas especiais para prazos fatais
                └── Enviar alerta urgente via WhatsApp
```

### Fluxo 4: AI Daily Briefing

```
Cron Job (07:00 AM, timezone do advogado)
        │
        ▼
n8n Workflow "Daily Briefing"
        │
        ├─── 1. GET /api/audiencias/hoje
        ├─── 2. GET /api/prazos/vencendo?horas=48
        ├─── 3. GET /api/entradas-ia/pendentes
        ├─── 4. GET /api/processos/movimentacoes-recentes
        │
        ▼ Compilar dados
GPT-4o: Gerar briefing personalizado
        │
        ├── "Bom dia, Dr. [Nome]! Você tem 3 tarefas críticas hoje..."
        ├── Resumo de audiências
        ├── Alertas de prazos
        └── Novos clientes/intimações
        │
        ▼ Enviar via WhatsApp + Salvar para Dashboard
```

---

## 🔐 Modelo de Segurança

### Multi-Tenancy: Tripla Proteção

```
Layer 1: JWT Claims
┌──────────────────────────────────────────────┐
│ JWT Token contém advogado_id como claim      │
│ TenantMiddleware extrai e valida             │
└──────────────────────────────────────────────┘
                    ↓
Layer 2: EF Core Global Query Filter
┌──────────────────────────────────────────────┐
│ Toda query é automaticamente filtrada:       │
│ WHERE advogado_id = @currentAdvogadoId       │
│ Aplicado em: Cliente, Processo, Audiência,   │
│ Prazo, Nota, EntradaIA                       │
└──────────────────────────────────────────────┘
                    ↓
Layer 3: PostgreSQL Row Level Security (RLS)
┌──────────────────────────────────────────────┐
│ Política no nível do banco de dados          │
│ Mesmo queries diretas são filtradas          │
│ Proteção contra bypass do ORM                │
└──────────────────────────────────────────────┘
```

---

## 🔒 Princípios de Engenharia (Revalidação)

### Regras obrigatórias para o desenvolvedor

- **Substituição por interfaces (DI):** Nenhum use case da Application layer deve instanciar concretamente provedores de e-mail, banco ou IA. Todas as dependências são injetadas via construtor e registradas no container (API ou Infrastructure).
- **DTO na API:** Nenhuma entidade do EF Core (Domain/Infrastructure) deve ser serializada diretamente na API ou retornada ao front-end. Usar sempre DTOs da Application layer (request/response por endpoint; mapeamento Entity → DTO nos Handlers ou mappers dedicados).
- **Validação antes da persistência:** Todo Command/Request que altera estado deve ter um Validator (FluentValidation) registrado. Falhas de validação retornam 400 com mensagens claras, sem atingir a camada de persistência.
- **Novas entidades tenant-aware:** Qualquer nova entidade que pertença a um advogado deve: (1) ter Global Query Filter no DbContext; (2) estar coberta por RLS no PostgreSQL; (3) ter pelo menos um teste de isolamento (Story 1.1.1).

### Audit Trail (Story 1.2.1)

- **Entidades auditáveis:** Processo, Prazo, Cliente, Audiencia, Nota possuem shadow properties no EF Core: `CreatedBy` e `LastModifiedBy` (Guid?, FK conceitual para Advogado). Não expostas na entidade; apenas no banco e em consultas de auditoria.
- **Significado:** `CreatedBy` = AdvogadoId do usuário que criou o registro; `LastModifiedBy` = AdvogadoId do usuário que fez a última alteração. Preenchidos automaticamente pelo `AuditSaveChangesInterceptor` a partir do `ITenantProvider` (usuário corrente). Em contexto sem usuário (ex.: webhook), permanecem null.
- **Tabela `audit_logs`:** Histórico de alterações para entidades críticas (Processo, Prazo). Colunas: entity_name, entity_id, action (Created/Updated/Deleted), advogado_id, old_values (JSON), new_values (JSON), changed_at. RLS garante que cada advogado só acessa seus próprios logs. Endpoint de consulta (ex.: `GET /api/audit-logs`) pode ser implementado em story futura.

### Integration Foundation (Story 1.4)

- **Webhook auth:** Rotas sob `/api/webhooks/` exigem assinatura HMAC-SHA256 no header `X-Webhook-Signature` (formato `sha256=&lt;hex&gt;`). Secret configurado em `Webhooks:N8nSecret`. Sem secret → 401; assinatura inválida → 403. No n8n, configurar o mesmo secret no webhook para assinar o body.
- **Cliente HTTP resiliente:** Named client `Agile360.AI` com Polly: retry (2 tentativas, backoff exponencial) para 5xx e timeout; circuit breaker (abre após 5 falhas, 30s). Timeout global 30s; User-Agent `Agile360-Backend/1.0`. BaseUrl e ApiKey (opcional) em `N8n:BaseUrl`, `N8n:ApiKey`.
- **AI Gateway:** Interface `IAiGatewayService` com `ExtractAsync<T>(AiExtractionRequest)`. Implementação `N8nAiGatewayService` chama o n8n via HTTP e retorna `AiExtractionResult<T>`. Ponto único para integração com n8n/IA; futura implementação pode delegar ao AIOS.
- **Variáveis de ambiente:** `Webhooks__N8nSecret`, `N8n__BaseUrl`, `N8n__ApiKey`.

---

## 📦 Decisões Arquiteturais

### ADR-001: Clean Architecture para .NET 9

**Contexto:** Precisamos de uma estrutura escalável e testável.
**Decisão:** Clean Architecture com 5 projetos (API, Application, Domain, Infrastructure, Shared).
**Consequências:** Separação clara de responsabilidades, testabilidade, mas mais boilerplate inicial.

### ADR-002: CQRS com MediatR

**Contexto:** Separar operações de leitura e escrita para melhor performance e clareza.
**Decisão:** Usar MediatR para Commands (escrita) e Queries (leitura).
**Consequências:** Código mais organizado, pipeline behaviors para cross-cutting concerns, mas overhead de classes.

### ADR-003: n8n como Orquestrador (não código .NET)

**Contexto:** Integrar WhatsApp, Email, GPT-4o e alertas requer fluxos complexos.
**Decisão:** Usar n8n para todos os fluxos de integração, mantendo a API .NET focada em CRUD e business logic.
**Consequências:** Separação clara de responsabilidades, GUI para editar fluxos, mas dependência de infraestrutura n8n.

### ADR-004: Supabase Auth + JWT (não IdentityServer)

**Contexto:** Precisamos de autenticação segura sem complexidade excessiva.
**Decisão:** Supabase Auth gerencia identidade, .NET valida JWT.
**Consequências:** Setup rápido, Auth UI pronto, mas dependência do Supabase para gestão de usuários.

### ADR-005: Multi-Tenancy por Coluna (não por Schema)

**Contexto:** Isolamento de dados por advogado.
**Decisão:** Coluna `advogado_id` em todas as tabelas + RLS.
**Consequências:** Simplicidade operacional, menor custo, mas requer disciplina para nunca esquecer o filtro.

### ADR-006: Storage de Documentos – Supabase Storage

**Contexto:** PDFs de processos e intimações precisam ser armazenados.
**Decisão:** Usar **Supabase Storage** com path `{advogado_id}/processos/{processo_id}/...`; políticas por tenant. Story futura em Epic 2 ou 7 para implementação.
**Referência:** [Gaps e Decisões](gaps-and-decisions.md#6-storage-de-documentos-pdfs-de-processos).

### ADR-007: Background Jobs – n8n primeiro; Hangfire sob avaliação

**Contexto:** Guardião de Prazos (Epic 5) precisa de execução agendada.
**Decisão:** **n8n** como orquestrador (cron chama API); não introduzir Hangfire na fundação. Avaliar Hangfire em story futura se surgirem jobs pesados dentro da API.
**Referência:** [Gaps e Decisões](gaps-and-decisions.md#7-mensageria--background-jobs-guardião-de-prazos--epic-5).

---

## 📎 Documentos Relacionados

- [Revalidação da Arquitetura](architecture-revalidation.md) – Revalidação do plano, User Stories e princípios (SOLID, DTO, multi-tenancy, UX).
- [Gaps e Decisões](gaps-and-decisions.md) – Tabela de verificação de gaps e decisões (testes, AI Gateway, CI/CD, auditoria, storage, TypeScript client, Hangfire).
- [Deployment](deployment.md) – Caminho para deploy (Azure, Cloudfy); variáveis de ambiente. (Criado na Story 1.5.)

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial architecture document | @architect (Aria) |

---

**Criado por:** Aria (@architect)
**Data:** 2026-02-20
