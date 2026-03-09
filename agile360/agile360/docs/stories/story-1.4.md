# Story 1.4: Integration Foundation – AI Gateway, n8n Webhooks & Resilient HTTP

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.4
**Sprint:** 1
**Priority:** 🟠 High
**Points:** 8
**Effort:** 6-10 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| Agile360 tem DNA de IA (n8n, assistentes) | PRD Agile360 | Backend precisa de camada padronizada para falar com agentes |
| n8n orquestra webhooks e GPT-4o | PRD Agile360 | API deve autenticar e receber webhooks do n8n; eventualmente disparar comandos |
| .NET 9 como backend | Story 1.1 | HttpClient com Refit/Polly para resiliência em chamadas a APIs de IA |

---

## 📋 User Story

**Como** desenvolvedor do Agile360,
**Quero** uma camada de integração padronizada para autenticar webhooks do n8n e fazer chamadas resilientes a APIs de IA e serviços externos,
**Para** que o backend converse de forma segura e confiável com o ecossistema n8n/AIOS e com provedores de IA (GPT-4o, etc.) sem duplicar lógica de retry, timeout e autenticação.

---

## 🎯 Objective

Implementar a **Integration Foundation** do Agile360: (1) autenticação e validação de webhooks vindos do n8n; (2) cliente HTTP resiliente (Polly + Refit opcional) para chamadas outbound a APIs de IA e ao n8n; (3) contrato de “AI Gateway” (interfaces e DTOs) para que comandos e respostas de IA sejam tratados de forma padronizada. Isso prepara as stories de Epic 3 (WhatsApp), Epic 4 (Intimações) e Epic 5 (Guardião de Prazos) e eventual integração com o framework AIOS.

---

## ✅ Tasks

### Phase 1: Webhook Security (n8n → .NET) (~2h)

- [ ] **1.1** Definir mecanismo de autenticação de webhooks n8n → API:
  - Opção A: Header fixo (ex.: `X-Webhook-Secret`) com secret compartilhado
  - Opção B: HMAC assinando o body (ex.: `X-Webhook-Signature: sha256=...`)
  - Documentar decisão em Architecture; recomendar HMAC para integridade
- [ ] **1.2** Criar `IWebhookSignatureValidator`:
  - `bool Validate(string payload, string signature, string secret)`
  - Suportar HMAC-SHA256 comparando signature com computed hash
- [ ] **1.3** Criar `WebhookAuthMiddleware` ou `WebhookAuthFilter`:
  - Aplicar apenas em rotas sob `/api/webhooks/`
  - Extrair signature do header; validar payload
  - Retornar 401 se secret não configurado; 403 se assinatura inválida
- [ ] **1.4** Configurar secret em `appsettings` / env:
  - `Webhooks:N8nSecret` (ou similar); nunca commitar valor real
- [ ] **1.5** Criar controller base ou atributo `[WebhookAuthorize]` para endpoints que recebem n8n
- [ ] **1.6** Documentar no README/Architecture: como configurar o mesmo secret no n8n

### Phase 2: Resilient HTTP Client (Polly + Refit) (~2.5h)

- [ ] **2.1** Instalar pacotes:
  - `Microsoft.Extensions.Http.Polly`
  - `Refit` e `Refit.HttpClientFactory` (opcional; pode usar HttpClient nomeado sem Refit)
- [ ] **2.2** Configurar `HttpClient` nomeado para “AI / External APIs”:
  - BaseAddress configurável (ex.: URL do n8n ou de um AI Gateway)
  - Timeout padrão (ex.: 30s)
  - User-Agent: `Agile360-Backend/1.0`
- [ ] **2.3** Configurar políticas Polly para esse cliente:
  - Retry: 2 retries com exponential backoff para 5xx e timeout
  - Circuit breaker: abrir após N falhas consecutivas; manter aberto 30s
  - Timeout por tentativa: 15s
- [ ] **2.4** Criar interface `IApiClientFactory` ou usar `IHttpClientFactory` diretamente:
  - Obter cliente nomeado "Agile360.AI" ou "Agile360.External"
  - Garantir que Polly está aplicado a esse nome
- [ ] **2.5** (Opcional) Definir contrato Refit para “chamada ao n8n”:
  - Ex.: `IN8nTriggerApi` com método `PostAsync(WebhookPayload payload)`
  - Facilita testes e evita magic strings de URL

### Phase 3: AI Gateway Abstração (~2h)

- [ ] **3.1** Criar pasta `Application/Integration/` ou `Application/AIGateway/`
- [ ] **3.2** Definir DTOs de “request/response” genéricos para chamadas de IA:
  - `AiExtractionRequest` (ex.: texto ou áudio URL, contexto)
  - `AiExtractionResult<T>` (sucesso + dados tipados ou erro)
  - Permitir extensão para diferentes fluxos (WhatsApp, Email, Briefing)
- [ ] **3.3** Criar interface `IAiGatewayService` (ou `IExternalAiService`):
  - Ex.: `Task<AiExtractionResult<T>> ExtractAsync<T>(AiExtractionRequest request, CancellationToken ct)`
  - Implementação pode ser “delegar ao n8n” (HTTP call) ou, no futuro, chamar AIOS
- [ ] **3.4** Implementar `N8nAiGatewayService`:
  - Recebe `IHttpClientFactory` e URL do workflow n8n
  - Serializa request; chama n8n via HTTP; deserializa response
  - Tratar timeout e falhas com Polly; mapear para `AiExtractionResult`
- [ ] **3.5** Registrar em DI: `IAiGatewayService` → `N8nAiGatewayService` (ou stub para dev)
- [ ] **3.6** Documentar em Architecture: “AI Gateway” é o ponto único de integração com n8n/IA; novas integrações (ex.: AIOS) implementam a mesma interface

### Phase 4: Outbound Auth ( .NET → n8n / APIs externas) (~1h)

- [ ] **4.1** Definir como a API .NET se autentica ao chamar n8n (se n8n exigir):
  - Ex.: API Key no header; ou mesmo webhook sem auth se rede interna
  - Configurar em `appsettings`: `N8n:BaseUrl`, `N8n:ApiKey` (opcional)
- [ ] **4.2** Configurar `HttpClient` para n8n com header de auth quando necessário
- [ ] **4.3** Garantir que chamadas de teste usem mock ou URL de dev (não prod)

### Phase 5: Documentação e Conventions (~0.5h)

- [ ] **5.1** Atualizar `docs/architecture/system-architecture.md`:
  - Seção “Integration Foundation”: webhook auth, AI Gateway, Polly
- [ ] **5.2** Adicionar exemplo de payload de webhook n8n → API (JSON) em docs ou Postman
- [ ] **5.3** Documentar variáveis de ambiente: `Webhooks__N8nSecret`, `N8n__BaseUrl`, `N8n__ApiKey`

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN um webhook do n8n com assinatura válida
WHEN o request chega em /api/webhooks/*
THEN a requisição é aceita e o handler é executado

GIVEN um webhook com assinatura inválida ou sem secret configurado
WHEN o request chega em /api/webhooks/*
THEN a API retorna 401 ou 403
AND o body do webhook não é processado

GIVEN o cliente HTTP "AI/External" está configurado
WHEN uma chamada a um serviço externo falha com 5xx ou timeout
THEN Polly aplica retry com backoff
AND após N falhas consecutivas o circuit breaker abre
AND não há throw não tratado (erro mapeado para Result ou log)

GIVEN IAiGatewayService está implementado
WHEN um use case chama ExtractAsync
THEN a chamada é feita ao n8n (ou stub) com timeout e retry
AND o resultado é mapeado para AiExtractionResult<T>
AND falhas são registradas em log e retornadas como resultado de erro
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Camada de integração e resiliência |
| Complexity | Medium | Webhook auth + Polly + abstração de gateway |
| Test Requirements | Unit + Integration | Validar assinatura; validar retry/circuit breaker com mock HTTP |
| Review Focus | Security, Resilience | Webhook secret e tratamento de falhas |

### Agent Assignment

| Role | Agent | Responsibility |
|------|-------|----------------|
| Primary | @dev | Implementação webhook auth, Polly, AI Gateway |
| Secondary | @architect | Revisar contrato IAiGatewayService e decisões de auth |
| Review | @qa | Testes de assinatura e comportamento sob falha |

### Focus Areas

- [ ] Webhook secret nunca em código; apenas config/env
- [ ] HMAC ou header validado em todo request de webhook
- [ ] Polly configurado para evitar cascata de falhas
- [ ] AI Gateway como único ponto de chamada a n8n/IA (facilita troca por AIOS depois)

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (API, DI, appsettings)

**Blocks:**
- Story 3.1–3.4: Evolution/n8n WhatsApp (webhook recebido na API; chamadas resilientes)
- Story 4.1–4.3: Monitor de Intimações (webhook + IA)
- Story 5.1–5.3: Guardião de Prazos (chamadas outbound se necessário)
- Integração futura com AIOS (mesmo contrato IAiGatewayService)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Secret de webhook vazado | High | Só em env/secrets; documentar rotação |
| n8n e .NET com expectativas diferentes de payload | Medium | Contrato DTO compartilhado; exemplo em doc |
| Circuit breaker muito agressivo | Low | Configurável; defaults conservadores |

---

## 📋 Definition of Done

- [ ] Webhook auth (HMAC ou header) implementado e aplicado em `/api/webhooks/*`
- [ ] HttpClient resiliente (Polly: retry + circuit breaker + timeout) configurado
- [ ] Interface IAiGatewayService e DTOs de AI request/result definidos
- [ ] N8nAiGatewayService implementado e registrado
- [ ] Documentação de architecture e env vars atualizada
- [ ] All acceptance criteria verified
- [ ] Testes unitários para validação de assinatura e para gateway (mock HTTP)

---

## 📝 Dev Notes

### Key Files

```
src/Agile360.API/
├── Middleware/
│   └── WebhookAuthMiddleware.cs   (ou Filter)
├── Controllers/
│   └── Webhooks/
│       └── N8nWebhookController.cs (base ou exemplo)

src/Agile360.Application/
├── Integration/
│   ├── IAiGatewayService.cs
│   ├── AiExtractionRequest.cs
│   ├── AiExtractionResult.cs
│   └── N8nAiGatewayService.cs     (ou em Infrastructure)

src/Agile360.Infrastructure/
├── Integration/
│   ├── WebhookSignatureValidator.cs
│   ├── N8nAiGatewayService.cs
│   └── HttpClientConfiguration.cs (Polly + named client)
```

### Autenticação .NET ↔ AIOS (futuro)

- Manter `IAiGatewayService` como abstração; uma implementação pode chamar AIOS em vez de n8n.
- Documentar em ADR: “Comandos para o framework AIOS” passarão por esse gateway ou por interface equivalente.

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story – Integration Foundation | @architect (Aria) |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20
