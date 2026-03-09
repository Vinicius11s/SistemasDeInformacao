# Agile360 – Fluxos de Usuário: Landing e Dashboard

**Versão:** 1.0.0  
**Autor:** Uma (@ux-design-expert)  
**Data:** 2026-02-21  
**Contexto:** [Frontend Architecture](../architecture/frontend-architecture.md) (seções 3 e 6), [Epic 1 – Identidade](../prd/epic-1-fundacao-infraestrutura.md)

---

## 1. Fluxos da Landing Page

### 1.1 Fluxo principal: Visitante → CTA → Login

```mermaid
flowchart LR
  A[Visitante em /] --> B[Lê Hero]
  B --> C[Scroll Benefícios]
  C --> D{Decisão}
  D -->|Começar / Acessar| E[/login]
  D -->|Cadastrar| F[/register]
  E --> G[Preenche e-mail/senha]
  G --> H{API Auth}
  H -->|Sucesso| I[Redirect /app]
  H -->|Erro| J[Feedback inline]
  J --> G
  F --> K[Preenche cadastro]
  K --> L{API Register}
  L -->|Sucesso| I
  L -->|Erro| M[Feedback inline]
  M --> K
```

**Passos detalhados:**

| # | Ação do usuário | Sistema | Feedback / UI |
|---|-----------------|--------|----------------|
| 1 | Acessa `/` | Exibe landing | Hero visível acima da dobra |
| 2 | Lê headline e subhead (Visão 360 e Esforço Zero) | — | Tipografia clara (Inter), contraste WCAG |
| 3 | Rola e lê blocos de benefícios | Scroll suave | Cada bloco com ícone + título + texto curto |
| 4 | Clica em **"Começar"** ou **"Acessar"** (CTA principal) | Navega para `/login` | Transição sem reload completo (SPA) |
| 4b | (Alternativa) Clica em **"Cadastrar"** na Navbar | Navega para `/register` | Idem |
| 5 | Preenche e-mail e senha no formulário de login | — | Campos com label, placeholder, estado de foco visível |
| 6 | Clica em **"Entrar"** | POST /api/auth/login | Botão em loading (spinner ou texto "Entrando…") |
| 7a | Resposta sucesso | Armazena token, redirect `/app` | Toast ou mensagem breve "Bem-vindo" (opcional) |
| 7b | Resposta erro (credenciais inválidas) | — | Mensagem inline abaixo do form: "E-mail ou senha inválidos." (aria-live) |
| 8 | (Se cadastro) Preenche nome, e-mail, senha, OAB etc. | POST /api/auth/register | Validação inline por campo; ao submeter, loading |
| 9 | Sucesso no cadastro | Token + redirect `/app` | Confirmação breve |

**CTAs na Landing:**

- **Primário (Hero):** "Começar agora" ou "Acessar o Agile360" → `/login`
- **Secundário (Hero):** "Cadastrar" → `/register` (se disponível)
- **Navbar:** "Login" (destaque), "Cadastrar" (link texto)

**Transição para login:** Sempre que o usuário escolhe "Começar" ou "Login", a rota muda para `/login`; o layout mantém a mesma Navbar (logo + links) e o conteúdo central passa a ser o formulário de login. Evitar múltiplos cliques; um único CTA claro acima da dobra.

---

### 1.2 Fluxo de recuperação de senha

| # | Ação | Sistema | Feedback |
|---|------|--------|---------|
| 1 | Em `/login`, clica em "Esqueci a senha" | Navega `/forgot-password` | — |
| 2 | Informa e-mail | POST /api/auth/forgot-password | Loading no botão |
| 3 | Sucesso | Mensagem: "Se o e-mail existir, você receberá um link…" | Toast ou região de status (não revelar se e-mail existe) |
| 4 | Usuário recebe e-mail e clica no link | Abre `/reset-password?token=...` | Form: nova senha + confirmação |
| 5 | Submete nova senha | POST /api/auth/reset-password | Sucesso → redirect `/login` + toast "Senha alterada" |

---

## 2. Fluxos do Dashboard (Aplicação Protegida)

### 2.1 Acesso e navegação

```mermaid
flowchart TD
  A[/app] --> B{Autenticado?}
  B -->|Não| C[Redirect /login?returnUrl=/app]
  B -->|Sim| D[Layout: Sidebar + Área de conteúdo]
  D --> E[Visão geral /app ou /app/dashboard]
  D --> F[/app/clientes]
  D --> G[/app/processos]
  D --> H[/app/audiencias]
  D --> I[/app/prazos]
  E --> F
  E --> G
  F --> G
  G --> H
  H --> I
```

**Regras:**

- Qualquer acesso a `/app` ou `/app/*` sem token válido → redirect para `/login` com `returnUrl` para restaurar destino após login.
- Layout do app: **sidebar fixa** (ou navbar superior) com itens: Início, Clientes, Processos, Audiências, Prazos; + usuário e "Sair" no topo ou rodapé da sidebar.
- Navegação por teclado: Tab entre itens da sidebar; Enter/Space ativa; setas opcional para subnavegação.

---

### 2.2 Fluxo: Listagem de clientes

| # | Ação do usuário | Sistema | Feedback / UI |
|---|-----------------|--------|----------------|
| 1 | Acessa `/app/clientes` | GET lista clientes (paginação) | Tabela ou cards; loading skeleton ou spinner |
| 2 | Vê lista (nome, CPF, e-mail, etc.) | — | Cabeçalhos de coluna clicáveis para ordenação (se API suportar) |
| 3 | (Opcional) Usa filtro ou busca | GET com query params | Debounce na busca; indicador de "Buscando…" |
| 4 | Clica em "Novo cliente" | Abre formulário (modal ou página) | Botão primário (laranja) |
| 5 | Preenche e submete | POST /api/clientes (ou equivalente) | Botão "Salvar" em loading |
| 6a | Sucesso | Fecha form, invalida lista, exibe lista atualizada | Toast "Cliente criado com sucesso" (aria-live) |
| 6b | Erro (validação ou API) | — | Mensagens inline por campo ou toast de erro |
| 7 | Clica em linha/card para editar | Abre form com dados | GET por id; loading no form |
| 8 | Edita e salva | PUT/PATCH | Idem 5–6; toast "Cliente atualizado" |

**Feedback imediato (princípio do PRD):** Toda ação (salvar, excluir) deve mostrar loading durante a requisição e, ao terminar, sucesso ou erro claro. Nunca deixar o advogado sem confirmação.

---

### 2.3 Fluxo: Listagem e formulário de processos

| # | Ação | Sistema | Feedback |
|---|------|--------|---------|
| 1 | Acessa `/app/processos` | GET lista processos (com filtro por cliente se aplicável) | Lista/ tabela; loading |
| 2 | Filtra por cliente ou status | GET com params | Mesmo padrão de listagem |
| 3 | "Novo processo" | Abre form | Campos: Cliente (select), Número do processo, Vara, Comarca, Status, etc. |
| 4 | Submete | POST processo | Loading → toast sucesso/erro; lista atualizada |
| 5 | Edita processo existente | PUT/PATCH | Idem; "Processo atualizado" |

---

### 2.4 Fluxo: Audiências

| # | Ação | Sistema | Feedback |
|---|------|--------|---------|
| 1 | Acessa `/app/audiencias` | GET audiências (lista ou calendário) | Vista lista ou calendário (semana/mês) |
| 2 | "Nova audiência" | Form: Processo, Data/hora, Local, Tipo, Status | Loading ao salvar |
| 3 | Salva | POST | Toast + atualização da vista |
| 4 | Clica em evento no calendário | Abre detalhe ou edição | Navegação ou modal |

---

### 2.5 Fluxo: Prazos

| # | Ação | Sistema | Feedback |
|---|------|--------|---------|
| 1 | Acessa `/app/prazos` | GET prazos (filtros: status, vencimento) | Lista com destaque visual para próximos/atrasados (cor ou ícone) |
| 2 | Filtra por status ou data | GET com params | Atualização da lista |
| 3 | "Novo prazo" | Form: Descrição, Tipo, Data de vencimento, Prioridade, Processo (opcional) | Loading ao salvar |
| 4 | Marca como concluído / altera status | PATCH | Toast + atualização imediata da linha |

---

### 2.6 Padrão global de feedback de ações

| Tipo de ação | Durante | Sucesso | Erro |
|--------------|---------|---------|------|
| Salvar (criar/editar) | Botão com loading (spinner ou "Salvando…") | Toast breve "X criado/atualizado"; lista/contexto atualizado | Toast ou mensagem inline com texto da API |
| Excluir | Botão "Excluir" desabilitado ou loading | Toast "X excluído"; item some da lista | Toast com mensagem de erro |
| Navegação | — | — | 401 → redirect login; 403 → toast "Sem permissão" |

Todas as mensagens de status (toast ou região de status) devem ser **acessíveis** (aria-live polite ou assertive para erros).

---

## 3. Resumo dos Pontos de Decisão

- **Landing:** Um CTA principal ("Começar" → login); Cadastro opcional na Navbar.
- **Login:** Gateway único para `/app`; returnUrl para devolver o usuário ao destino após autenticar.
- **Dashboard:** Navegação lateral (sidebar) estável; listagens com loading e atualização pós-mutação; formulários com validação e feedback inline + toast.
- **Acessibilidade:** Contraste (tema escuro), foco visível, aria-live para feedback; navegação por teclado em sidebar e formulários.

---

**Referências:** [Frontend Architecture §3 e §6](../architecture/frontend-architecture.md), [Epic 1 – Identidade e princípio de feedback](../prd/epic-1-fundacao-infraestrutura.md).

— Uma (@ux-design-expert)
