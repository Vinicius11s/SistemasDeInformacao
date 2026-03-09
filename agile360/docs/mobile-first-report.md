# Relatório Mobile-First — Agile360

## Objetivo
Garantir que o Agile360 seja totalmente utilizável em celular (360×800 Android, 390×844 iPhone), com foco em advogados que consultam compromissos e processos entre audiências.

---

## 1. Componentes ajustados

### 1.1 Dashboard (DashboardHome)
- **Resumo de Hoje (GetHojeAsync):** Grid de indicadores alterado de `grid-cols-2 lg:grid-cols-4` para **`grid-cols-1 sm:grid-cols-2 lg:grid-cols-4`** — em telas &lt; 640px exibe 1 coluna; entre 640–1024px, 2 colunas.
- **Agenda Semanal (compromissos_semana):** Calendário Seg–Sex com 5 colunas passou a ter **scroll horizontal** em viewports estreitas (`overflow-x-auto` + `min-w-[320px]` no grid), evitando esmagar as colunas em 360px.
- **Grid inferior (Processos + Prazos):** Mantido `grid-cols-1 lg:grid-cols-2` — já uma coluna no mobile.

### 1.2 Navegação (DashboardLayout)
- **Sidebar:** Ocultada em **&lt; 768px** (`hidden md:flex`).
- **Mobile:**
  - **Top bar** com logo + botão **hambúrguer** (44×44px) que abre um **drawer** com todos os itens de menu, perfil, tema e sair.
  - **Barra inferior fixa** com 5 itens (Painel, Clientes, Processos, Compromissos, Prazos), cada um com **min 44×44px** e ícone + label.
- **Conteúdo:** `main` com `p-4 pb-24 md:p-7` e **safe-area** (`padding-left/right/bottom` usando `env(safe-area-inset-*)`) para notch e gestos.

### 1.3 Compromissos (Audiencias)
- **Problema:** Tabela com muitas colunas (Tipo, Data, Hora, Cliente, Processo, Status, Ações) vazava no celular.
- **Solução:**
  - **&lt; 768px:** Lista de **cards** (um por compromisso) com: tipo, data/hora formatada, cliente, processo, status e botões Editar/Excluir com **min-h-[44px]**.
  - **≥ 768px:** Tabela mantida (`hidden md:block`).
- Cabeçalho (título + “+ Novo compromisso”) empilha no mobile; botão full-width com 44px de altura.

### 1.4 Clientes
- **Lista:** Mesmo padrão — **cards no mobile** (&lt; 768px), tabela no desktop. Cada card mostra nome, tipo (PF/PJ), CPF/CNPJ, telefone, cidade/UF e botões Editar/Excluir (44px).
- **Formulário (Cadastro de Cliente):**
  - Tipo de cliente (Pessoa Física / Jurídica): opções em **min-h-[44px] flex-1** e `touch-manipulation`.
  - Inputs e selects já utilizam `--input-min-height: 44px` e `min-h-[44px]` nos selects do modal.
- Cabeçalho com “Cadastro em massa” e “+ Novo cliente” empilha; botões 44px e full-width no mobile.

### 1.5 Processos
- Lista: **cards no mobile** com número do processo, cliente, assunto/tribunal, status e ações (Editar/Excluir 44px). Tabela no desktop.
- Cabeçalho responsivo; botão “+ Novo processo” 44px e full-width no mobile.

### 1.6 Prazos
- Lista: **cards no mobile** com título, tipo, cliente, vencimento, dias/urgência, prioridade, status e ações (44px). Borda esquerda colorida por urgência mantida.
- Filtros de status (Todos, Pendente, etc.): botões com **min-h-[44px] min-w-[44px]** e `touch-manipulation`.
- Cabeçalho e “+ Novo prazo” responsivos.

---

## 2. UI/UX

### 2.1 Alvos de toque
- **Mínimo 44×44px** em botões e links interativos usados no mobile:
  - Itens do menu (sidebar e drawer), botão hambúrguer, itens da barra inferior.
  - Botões Editar/Excluir em todos os cards (mobile).
  - Botões primários (Novo cliente, Novo processo, Novo compromisso, Novo prazo).
  - Toggle tema e Sair no drawer; filtros de status em Prazos.
- **Inputs e selects:** `--input-min-height: 44px` em `index.css`; componentes `Input` e selects dos modais com `min-h-[44px]`.
- **Fallback global:** Em `index.css`, regra `@media (pointer: coarse)` para garantir `min-height/min-width: 44px` em botões que não tenham já classe explícita.

### 2.2 Contraste e espaçamento
- Contraste segue o Design System (text-heading, text, text-muted); sem alteração.
- **Espaçamento lateral:** Conteúdo com `p-4` no mobile e `max(1rem, env(safe-area-inset-left/right))` no `main` para não colar nas bordas em dispositivos com notch/gestos.
- **Padding inferior:** `pb-24` no mobile para o conteúdo não ficar atrás da barra inferior; soma com `env(safe-area-inset-bottom)`.

### 2.3 Imagens e ícones
- Ícones: **Lucide React** (SVG inline), leves e escaláveis — adequados para 3G/4G.
- Não há imagens pesadas nas telas ajustadas; se no futuro houver imagens (ex.: avatar, anexos), recomenda-se `loading="lazy"` e dimensões explícitas.

---

## 3. Tarefas técnicas realizadas

| Tarefa | Implementação |
|--------|----------------|
| Media Queries / Tailwind | Breakpoint **768px** (`md:`) para sidebar, tabelas vs cards; **640px** (`sm:`) para grid 2 colunas no dashboard. |
| Grid 3–4 colunas → 1 coluna | Dashboard: `grid-cols-1 sm:grid-cols-2 lg:grid-cols-4`. Demais listas: 1 coluna de cards no mobile. |
| Testes de resolução | Layout preparado para 360×800 e 390×844; uso de `viewport-fit=cover` e safe-area para notch. |

---

## 4. Resumo de arquivos alterados

| Arquivo | Alterações |
|---------|------------|
| `frontend/src/layouts/DashboardLayout.tsx` | Sidebar oculta em &lt;768px; top bar + hambúrguer + drawer; barra inferior; safe-area no `main`. |
| `frontend/src/pages/DashboardHome.tsx` | Grid 1 col mobile; calendário semanal com scroll horizontal. |
| `frontend/src/pages/Audiencias.tsx` | Lista de cards no mobile; tabela no desktop; cabeçalho responsivo. |
| `frontend/src/pages/Clientes.tsx` | Lista de cards no mobile; tabela no desktop; cabeçalho e tipo de cliente com 44px. |
| `frontend/src/pages/Processos.tsx` | Lista de cards no mobile; tabela no desktop; cabeçalho responsivo. |
| `frontend/src/pages/Prazos.tsx` | Lista de cards no mobile; tabela no desktop; cabeçalho e filtros com 44px. |
| `frontend/src/index.css` | Regra `@media (pointer: coarse)` para alvos de toque mínimos. |
| `frontend/index.html` | `viewport-fit=cover` no viewport. |

---

## 5. Resultado esperado

- **Nenhum componente “quebrando” no mobile:** tabelas substituídas por cards em &lt;768px; sidebar por hambúrguer + barra inferior; calendário com scroll horizontal.
- **Formulário de Cadastro de Cliente e lista de Compromissos** utilizáveis com uma mão: botões e campos com 44px; cards roláveis sem scroll horizontal forçado.
- **Tabela de Compromissos** no celular exibida como **cards** por compromisso (Data, Hora, Tipo, Cliente, Processo, Status, Ações), sem vazar para fora da tela.
