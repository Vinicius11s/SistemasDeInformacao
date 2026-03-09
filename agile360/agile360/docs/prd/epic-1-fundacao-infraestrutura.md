# Epic 1: Fundação e Infraestrutura do Agile360

**Produto:** Agile360 – Visão Jurídica Inteligente
**Versão:** 1.0.0
**Status:** ⚪ Ready
**Criado por:** Aria (@architect)
**Data:** 2026-02-20

---

## 📋 Visão Geral do Produto

O **Agile360** é um CRM SaaS jurídico focado em **"Visão 360 e Esforço Zero"**, projetado para advogados modernos que buscam agilidade e segurança jurídica. O sistema centraliza a vida profissional do advogado em um fluxo automatizado, desde a captura de clientes via WhatsApp até o monitoramento preditivo de prazos.

### Princípio de UX (visão de produto)

- **Feedback imediato:** Toda ação do usuário (salvar, excluir, enviar) deve ter feedback visual imediato (loading + sucesso/erro). O advogado nunca deve ficar sem confirmação. (Refinado em Epic 6/7 – Dashboard e Frontend.)

### Identidade Visual

- **Tema:** Dark moderno
- **Cores primárias:** Prata/Cinza metálico (#C0C0C0 / #A8A8A8) e Laranja vibrante (#FF6B00 / #FF8C38)
- **Tipografia:** Inter / JetBrains Mono (code)

---

## 🏗️ Stack Tecnológica

| Camada | Tecnologia | Justificativa |
|--------|-----------|---------------|
| **Backend API** | .NET 9 (C#) | Performance, tipagem forte, ecossistema enterprise |
| **Banco de Dados** | Supabase (PostgreSQL) | Multi-Tenancy, Auth nativo, Realtime, Storage |
| **Orquestração** | n8n | Workflows visuais, Webhooks, integração fácil |
| **IA** | GPT-4o (via n8n) | Extração de dados, NLP, resumos inteligentes |
| **WhatsApp** | Evolution API | API estável, self-hosted, multi-device |
| **Calendário** | Google Agenda API | Sincronização bidirecional |
| **E-mail** | IMAP/SMTP | Monitoramento de intimações |
| **Frontend** | A definir (React/Next.js ou Blazor) | Dashboard desktop-first |

---

## 📊 Mapa de Epics

### Sprint 1-2: Fundação

| Epic | Nome | Prioridade | Stories |
|------|------|-----------|---------|
| **Epic 1** | Fundação e Infraestrutura | 🔴 Critical | 1.1 – 1.5, 1.1.1, 1.2.1 |
| **Epic 2** | CRM Jurídico Core | 🔴 Critical | 2.1 – 2.4 |

### Sprint 3-4: Integrações

| Epic | Nome | Prioridade | Stories |
|------|------|-----------|---------|
| **Epic 3** | Captura Neural via WhatsApp | 🟠 High | 3.1 – 3.4 |
| **Epic 4** | Monitor de Intimações | 🟠 High | 4.1 – 4.3 |
| **Epic 5** | Guardião de Prazos | 🟠 High | 5.1 – 5.3 |

### Sprint 5-6: Dashboard & UX

| Epic | Nome | Prioridade | Stories |
|------|------|-----------|---------|
| **Epic 6** | Dashboard Hub Central | 🟠 High | 6.1 – 6.5 |
| **Epic 7** | Frontend & Dark Theme | 🟡 Medium | 7.1 – 7.3 |

---

## 📋 Detalhamento dos Epics

### Epic 1: Fundação e Infraestrutura

> Base técnica do projeto: API, banco de dados, autenticação, testes, integração, CI/CD e auditoria.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **1.1** | Project Scaffolding – .NET 9 API + Supabase | 8 | 🔴 Critical |
| **1.1.1** | Test Foundation – Unit & Integration (xUnit, NSubstitute) | 5 | 🔴 Critical |
| **1.2** | Multi-Tenancy Architecture & Database Schema | 13 | 🔴 Critical |
| **1.2.1** | Audit Trail Foundation – Shadow Properties & Audit Log | 5 | 🟠 High |
| **1.3** | Authentication & Authorization (JWT + RLS) | 8 | 🔴 Critical |
| **1.4** | Integration Foundation – AI Gateway, n8n Webhooks & Resilient HTTP | 8 | 🟠 High |
| **1.5** | CI/CD Pipeline – GitHub Actions & Deployment Path | 5 | 🟠 High |

### Epic 2: CRM Jurídico Core

> CRUD completo de entidades jurídicas vinculadas ao advogado.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **2.1** | CRUD Clientes (Clients Management) | 5 | 🔴 Critical |
| **2.2** | CRUD Processos (Cases/Lawsuits Management) | 8 | 🔴 Critical |
| **2.3** | CRUD Audiências (Hearings Management) | 5 | 🟠 High |
| **2.4** | Vinculação Advogado-Cliente-Processo-Audiência | 5 | 🟠 High |

### Epic 3: Captura Neural via WhatsApp

> Extração inteligente de dados de clientes via mensagens WhatsApp.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **3.1** | Evolution API Integration & Webhook Setup | 8 | 🟠 High |
| **3.2** | n8n Workflow – WhatsApp Message Processing | 8 | 🟠 High |
| **3.3** | GPT-4o Data Extraction (Nome, CPF, Caso) | 13 | 🟠 High |
| **3.4** | Contact Cleaning & Data Normalization | 3 | 🟡 Medium |

### Epic 4: Monitor de Intimações

> Leitura automática de e-mails para identificar intimações e criar audiências.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **4.1** | Email Integration Setup (IMAP Monitoring) | 5 | 🟠 High |
| **4.2** | AI-Powered Intimação Extraction | 13 | 🟠 High |
| **4.3** | Automatic Hearing/Deadline Creation from Email | 8 | 🟠 High |

### Epic 5: Guardião de Prazos

> Monitoramento preditivo de prazos com alertas via WhatsApp.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **5.1** | Deadline Monitoring Engine | 8 | 🟠 High |
| **5.2** | WhatsApp Alert System (via Evolution API) | 5 | 🟠 High |
| **5.3** | Predictive Analytics & Risk Scoring | 13 | 🟡 Medium |

### Epic 6: Dashboard Hub Central

> Página principal do advogado com visão 360 do dia.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **6.1** | AI Daily Briefing Component | 8 | 🟠 High |
| **6.2** | Inbox de IA (WhatsApp + Email Entries) | 8 | 🟠 High |
| **6.3** | Audiências & Prazos View (Contagem Regressiva) | 5 | 🟠 High |
| **6.4** | Calendário Semanal Interativo | 8 | 🟡 Medium |
| **6.5** | Processos Recentes & Bloco de Notas | 5 | 🟡 Medium |

### Epic 7: Frontend & Dark Theme

> Interface desktop-first com tema dark moderno.

| Story | Título | Points | Prioridade |
|-------|--------|--------|-----------|
| **7.1** | Dashboard Layout & Dark Theme Setup | 8 | 🟡 Medium |
| **7.2** | Component Library (Design System Agile360) | 13 | 🟡 Medium |
| **7.3** | Responsive Design & Mobile Optimization | 5 | 🟢 Low |

---

## 📊 Resumo de Estimativas

| Epic | Stories | Total Points | Sprints Est. |
|------|---------|-------------|-------------|
| Epic 1 | 7 | 52 | 2.5 |
| Epic 2 | 4 | 23 | 1.5 |
| Epic 3 | 4 | 32 | 2 |
| Epic 4 | 3 | 26 | 1.5 |
| Epic 5 | 3 | 26 | 1.5 |
| Epic 6 | 5 | 34 | 2 |
| Epic 7 | 3 | 26 | 1.5 |
| **Total** | **29** | **219** | **~12.5** |

---

## 🔗 Dependências entre Epics

```
Epic 1 (Fundação) ──────────────────────────────┐
    ↓                                            │
Epic 2 (CRM Core) ──────────────────────────┐   │
    ↓                    ↓                   │   │
Epic 3 (WhatsApp)   Epic 4 (Intimações)     │   │
    ↓                    ↓                   │   │
    └──────┬─────────────┘                   │   │
           ↓                                 │   │
    Epic 5 (Guardião de Prazos)              │   │
           ↓                                 │   │
    Epic 6 (Dashboard) ←────────────────────-┘   │
           ↓                                     │
    Epic 7 (Frontend) ←─────────────────────────-┘
```

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Epic overview creation | @architect (Aria) |

---

**Criado por:** Aria (@architect)
**Data:** 2026-02-20
