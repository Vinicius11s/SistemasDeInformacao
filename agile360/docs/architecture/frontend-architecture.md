# Agile360 – Frontend Architecture (Visão e Especificação)

**Versão:** 1.0.0  
**Status:** 🏗️ Especificação para execução  
**Criado por:** Aria (@architect)  
**Data:** 2026-02-21  

Este documento concebe a arquitetura do frontend do Agile360 sob a ótica da **Visão 360 e Esforço Zero**, com foco em advogados: landing persuasiva, autenticação como gateway e dashboard CRM com estado e performance holísticos.

---

## 1. Objetivos e Princípios

| Objetivo | Princípio |
|----------|-----------|
| **Conversão na landing** | Página de vendas persuasiva, clara e orientada a benefícios para o advogado |
| **Segurança e clareza** | Autenticação como único gateway para a aplicação protegida; Navbar consistente |
| **Produtividade no CRM** | Dashboard que suporta compromissos, audiências e gestão de clientes sem fricção |
| **Performance holística** | Estado global coerente, carregamento progressivo e UX responsiva (desktop-first) |

---

## 2. Stack e Escopo Técnico (Recomendação)

| Decisão | Recomendação | Justificativa |
|---------|--------------|---------------|
| **Framework** | React 18+ com TypeScript | Ecossistema maduro, tipagem alinhada à API .NET, fácil geração de cliente a partir do OpenAPI |
| **Roteamento** | React Router v6 (ou framework com file-based routing) | Rotas explícitas para `/`, `/login`, `/app/*` |
| **Estado global** | Zustand ou React Context + useReducer (evitar over-engineering) | Suporte a: sessão do usuário (JWT), dados do dashboard (clientes, processos, audiências, prazos) com invalidação/refetch |
| **Chamadas à API** | Cliente gerado (NSwag/OpenAPI) + fetch ou React Query (TanStack Query) | Cache, retry e invalidação por query key; alinhamento com contratos da API |
| **UI e tema** | Componentes com design system (Epic 1 PRD: dark theme, prata/cinza + laranja, Inter/JetBrains Mono) | Consistência visual; acessibilidade (contraste, foco) |
| **Build** | Vite ou Next.js (SSR opcional para landing) | Build rápido; Next.js permite SSR da landing para SEO se necessário |

*Nota: Blazor permanece alternativa válida se a equipe preferir stack .NET full; a arquitetura de fluxos e estado permanece aplicável.*

---

## 3. Visão de Telas e Fluxos

### 3.1 Index (Landing Page)

- **Objetivo:** Página de vendas persuasiva para advogados que buscam simplificar a gestão (clientes, processos, prazos, audiências).
- **Conteúdo sugerido:**
  - Hero com proposta de valor clara (“Visão 360 e Esforço Zero”).
  - Benefícios em blocos (gestão de clientes, prazos, audiências, integração WhatsApp, segurança jurídica).
  - Social proof ou indicadores (opcional em MVP).
  - CTA principal: “Começar” / “Acessar” que leva ao **fluxo de login** (ou cadastro).
- **Rota:** `/` (pública).
- **Performance:** Assets estáticos otimizados; lazy load de seções abaixo da dobra se necessário.

### 3.2 Fluxo de Autenticação (Gateway)

- **Navbar (global):**
  - Na landing: logo, links de benefícios, botão **“Login”** (e opcional “Cadastrar”).
  - Após login: mesmo layout com botão **“Sair”** e (opcional) nome do usuário; link para o Dashboard.
- **Login como gateway:**  
  O usuário não acessa o Dashboard sem autenticar. Após login bem-sucedido:
  - Armazenar token (e refresh token) de forma segura (httpOnly cookie ou memory + refresh em backend; evitar localStorage para refresh se possível).
  - Redirecionar para `/app` (ou rota raiz do app protegido).
- **Rotas:**
  - `/login` – formulário de login (e-mail/senha).
  - `/register` – cadastro (se disponível na API).
  - `/forgot-password`, `/reset-password` – fluxos de recuperação (conforme API).
- **Proteção:** Rotas sob `/app/*` exigem autenticação; caso contrário, redirecionar para `/login` com returnUrl.

### 3.3 Dashboard do CRM (Aplicação Protegida)

- **Objetivo:** Visualização e gestão de compromissos, audiências e clientes com estado coerente e performance adequada.
- **Rota base:** `/app` (layout com sidebar ou navbar interna + área de conteúdo).
- **Módulos / sub-rotas (sugestão):**
  - `/app` ou `/app/dashboard` – visão geral (resumo de prazos, próximas audiências, alertas).
  - `/app/clientes` – listagem, filtros, criação/edição de clientes.
  - `/app/processos` – listagem por cliente ou geral; criação/edição de processos.
  - `/app/audiencias` – calendário ou lista de audiências; vínculo com processos.
  - `/app/prazos` – lista de prazos com filtros (status, vencimento); alertas visuais.
- **Estado recomendado:**
  - **Sessão:** usuário logado (advogado), token, refresh lógico (ex.: React Query para `GET /api/auth/me` com refetch on window focus).
  - **Dados de domínio:** clientes, processos, audiências, prazos – gerenciados por **React Query** (ou equivalente) com query keys por recurso e por tenant implícito (token já envia advogado no JWT). Invalidação após mutações (criar/editar/remover).
- **Performance:**
  - Code splitting por rota (lazy load de `/app/clientes`, `/app/processos`, etc.).
  - Paginação ou virtualização em listas grandes.
  - Evitar over-fetch: endpoints que retornem apenas campos necessários (API já preparada com DTOs).

---

## 4. Integração com a API (.NET)

| Aspecto | Especificação |
|---------|----------------|
| **Base URL** | Configurável (env); em dev `http://localhost:5000` ou porta da API. |
| **Autenticação** | Bearer token no header `Authorization` para rotas protegidas; refresh via `POST /api/auth/refresh`. |
| **Contratos** | Cliente TypeScript gerado a partir do Swagger/OpenAPI (NSwag ou OpenAPI Generator); DTOs e `ApiResponse<T>` refletidos em tipos. |
| **Erros** | Tratar `ApiResponse.Fail` e status HTTP (401 → redirect login; 403 → mensagem; 4xx/5xx → feedback ao usuário). |
| **CORS** | API já configurada com origens permitidas; frontend deve usar a mesma origem ou URL configurada. |

---

## 5. Segurança no Frontend

- Não armazenar tokens em localStorage se houver risco de XSS; preferir memory + refresh em backend ou httpOnly cookie (se a API suportar).
- Validar que todas as chamadas a `/app/*` incluam o token; interceptors (axios/fetch) para adicionar header e tratar 401 (refresh ou redirect).
- Formulários: validação client-side alinhada aos validators da API (FluentValidation); mensagens de erro claras.

---

## 6. Acessibilidade e UX

- Contraste adequado (tema escuro: prata/cinza + laranja conforme PRD).
- Navegação por teclado e foco visível.
- Feedback imediato em todas as ações (loading, sucesso, erro) conforme princípio de UX do Epic 1.
- Mensagens de erro acessíveis (aria-live ou região de status).

---

## 7. Próximos Passos (Handoff)

1. **@ux-design-expert (Uma):** Desenhar fluxos de usuário detalhados da landing (hero, CTAs, benefícios) e do dashboard (navegação, listagens, formulários de clientes/processos/audiências/prazos); wireframes ou protótipos de alta fidelidade com o design system (dark theme, cores, tipografia).
2. **@data-engineer (Dara):** Revisar e otimizar queries e índices no Supabase que alimentam os endpoints consumidos pelo dashboard; validar RLS e integração dos DTOs com o schema; performance de listagens e filtros.
3. **@dev:** Implementar a aplicação frontend conforme esta especificação e os entregáveis da Uma (componentes, rotas, estado, integração com a API); implementar ou ajustar endpoints na API .NET conforme necessário para suportar o dashboard (se ainda não existirem CRUDs para clientes, processos, audiências, prazos).

---

## 8. Referências

- [System Architecture](system-architecture.md) – visão geral do sistema e API.
- [Epic 1 – Fundação](..//prd/epic-1-fundacao-infraestrutura.md) – identidade visual e stack.
- [Deployment](deployment.md) – variáveis de ambiente e base URL da API.

---

**Criado por:** Aria (@architect)  
**Assinatura:** — Aria, arquitetando o futuro 🏗️
