# Agile360 – Design System Spec (Tema Escuro)

**Versão:** 1.0.0  
**Autor:** Uma (@ux-design-expert)  
**Data:** 2026-02-21  
**Base:** [Epic 1 – Identidade Visual](../prd/epic-1-fundacao-infraestrutura.md)

---

## 1. Tokens de Cor

### 1.1 Paleta principal (PRD)

| Token | Uso | Hex | Contraste (BG escuro) |
|-------|-----|-----|------------------------|
| **Primary (Laranja)** | CTAs, links principais, destaques | `#FF6B00` | — |
| **Primary Hover** | Hover em botão primário | `#FF8C38` | — |
| **Silver** | Texto principal em dark | `#C0C0C0` | AAA em #1a1a1a |
| **Silver Muted** | Texto secundário | `#A8A8A8` | AA em #1a1a1a |
| **Background** | Fundo da aplicação | `#1a1a1a` | — |
| **Surface** | Cards, sidebars, inputs | `#252525` | — |
| **Border** | Bordas sutis | `#3a3a3a` | — |
| **Error** | Mensagens de erro, validação | `#E53935` ou `#FF5252` | AA em #1a1a1a |
| **Success** | Confirmação, sucesso | `#4CAF50` | AA em #1a1a1a |
| **Focus ring** | Contorno de foco (acessibilidade) | `#FF8C38` 2px solid | Visível em todos os controles |

### 1.2 Uso garantido de contraste (WCAG AA mínimo)

- **Texto principal** sobre `#1a1a1a`: usar `#C0C0C0` (Silver).
- **Texto secundário**: `#A8A8A8`.
- **Botão primário**: fundo `#FF6B00`, texto branco `#FFFFFF` (contraste suficiente).
- **Links**: `#FF6B00`; no foco: outline laranja.
- **Inputs**: fundo `#252525`, borda `#3a3a3a`, texto `#C0C0C0`; placeholder `#A8A8A8`.

---

## 2. Tipografia

| Token | Fonte | Uso | Tamanho (exemplo) | Peso |
|-------|--------|-----|-------------------|------|
| **Font Family UI** | Inter | Textos, labels, botões, navegação | 16px base | 400 normal, 600 semibold, 700 bold |
| **Font Family Code** | JetBrains Mono | Números de processo, CPF, códigos | 14px | 400 |
| **Heading 1** | Inter | Hero, títulos de página | 2.5rem (40px) | 700 |
| **Heading 2** | Inter | Seções (benefícios, módulos) | 1.75rem (28px) | 600 |
| **Heading 3** | Inter | Cards, subseções | 1.25rem (20px) | 600 |
| **Body** | Inter | Parágrafos, listas | 1rem (16px) | 400 |
| **Small** | Inter | Legendas, hints | 0.875rem (14px) | 400 |

Line-height sugerido: 1.5 para body; 1.2 para headings.

---

## 3. Componentes (Atoms → Molecules)

### 3.1 Botões

- **Primário:** fundo `#FF6B00`, texto branco, border-radius 8px, padding 12px 24px. Hover: `#FF8C38`. Focus: outline 2px `#FF8C38` offset 2px.
- **Secundário:** fundo transparente, borda `#3a3a3a`, texto `#C0C0C0`. Hover: borda `#A8A8A8`, fundo `#252525`.
- **Ghost (link):** texto `#FF6B00`, sem borda. Hover: sublinhado ou `#FF8C38`.

Todos os botões: estado de loading (spinner ou texto "Salvando…") e disabled (opacidade reduzida, cursor not-allowed).

### 3.2 Inputs

- **Texto:** fundo `#252525`, borda `#3a3a3a`, altura mínima 44px (touch target). Focus: borda `#FF6B00` ou focus ring. Label acima do campo, cor `#A8A8A8`. Mensagem de erro abaixo, cor `#FF5252`, aria-describedby no input.
- **Checkbox / Radio:** mesmo esquema de cores; área de clique ampla; focus ring visível.

### 3.3 Cards (Surface)

- Fundo `#252525`, borda `#3a3a3a`, border-radius 12px, padding 24px. Sombreamento opcional (box-shadow sutil) para elevação.

### 3.4 Navbar / Sidebar

- Fundo `#252525` ou `#1f1f1f`; itens com texto `#C0C0C0`; item ativo: texto `#FF6B00` e/ou barra lateral laranja. Hover: fundo `#2a2a2a`. Logo e "Sair" sempre visíveis.

### 3.5 Toast / Feedback

- Toast de sucesso: fundo `#252525`, borda esquerda `#4CAF50`, ícone check. Erro: borda `#E53935`. Posição: canto superior direito ou inferior; aria-live="polite" (sucesso) ou "assertive" (erro). Auto-dismiss 4–5s ou botão fechar.

---

## 4. Acessibilidade (Checklist)

- **Contraste:** Todo texto e ícones com contraste mínimo WCAG AA (4.5:1 para texto normal; 3:1 para grande).
- **Foco:** Todo elemento interativo com outline visível no foco (2px `#FF8C38`); nunca `outline: none` sem alternativa.
- **Navegação por teclado:** Tab order lógico; Enter/Space ativam botões e links; Escape fecha modais.
- **Labels:** Todos os inputs com `<label>` associado (for/id) ou aria-label.
- **Erros:** Mensagens de erro vinculadas ao campo (aria-describedby, aria-invalid quando aplicável).
- **Status ao vivo:** Região aria-live para toasts e mensagens de sucesso/erro após ações.
- **Tamanhos de alvo:** Mínimo 44x44px para botões e itens de lista clicáveis (touch e mouse).

---

## 5. Wireframes de Referência (Estrutura)

### 5.1 Landing – Hero + Benefícios (estrutura)

```
+------------------------------------------------------------------+
|  [Logo Agile360]                    Benefícios   Login  Cadastrar |
+------------------------------------------------------------------+
|                                                                    |
|     Visão 360 e Esforço Zero                                        |
|     CRM jurídico que centraliza clientes, processos e prazos.     |
|                                                                    |
|     [  Começar agora  ]    [ Cadastrar ]                           |
|                                                                    |
+------------------------------------------------------------------+
|  BENEFÍCIOS                                                        |
|  +----------------+  +----------------+  +----------------+         |
|  | ícone          |  | ícone          |  | ícone          |         |
|  | Gestão de      |  | Prazos e       |  | Audiências e   |         |
|  | clientes       |  | alertas        |  | calendário     |         |
|  +----------------+  +----------------+  +----------------+         |
|  +----------------+  +----------------+                             |
|  | WhatsApp + IA  |  | Segurança      |                             |
|  +----------------+  +----------------+                             |
+------------------------------------------------------------------+
```

### 5.2 Dashboard – Layout com Sidebar

```
+----------+--------------------------------------------------------+
| Logo     |  Agile360 – Dashboard                    [Nome] [Sair]  |
+----------+--------------------------------------------------------+
| Início   |                                                        |
| Clientes |   Título da página (ex.: Clientes)                     |
| Processos|   [ + Novo cliente ]     [ Busca... ]                  |
| Audiências|  +--------------------------------------------------+ |
| Prazos   |  | Nome        | CPF    | E-mail    | Ações          | |
|          |  | Maria Silva | ***... | m@...     | [Editar]       | |
|          |  | João Santos | ***... | j@...     | [Editar]       | |
|          |  +--------------------------------------------------+ |
| __________|  (Toast: "Cliente criado com sucesso" - canto superior)|
| Sair      |                                                        |
+----------+--------------------------------------------------------+
```

### 5.3 Formulário (modal ou página) – Novo cliente

```
+------------------------------------------+
|  Novo cliente                       [X]  |
+------------------------------------------+
|  Nome *                                  |
|  [________________________]              |
|  CPF                                     |
|  [________________________]              |
|  E-mail                                  |
|  [________________________]              |
|  Telefone                                |
|  [________________________]              |
|  Origem                                  |
|  [ Manual        ▼ ]                     |
|                                          |
|  [ Cancelar ]    [ Salvar ]  (loading?)  |
+------------------------------------------+
```

---

## 6. Arquivos de Referência para Implementação

- **Cores e tipografia:** Usar os tokens acima em CSS variables ou tema (Tailwind, Chakra, MUI, etc.).
- **Componentes:** Implementar atoms (Button, Input, Label, Card) seguindo os estilos; depois molecules (FormField, Toast, NavItem).
- **Páginas:** Templates conforme wireframes; conteúdo real vindo da API.

Para protótipos visuais de alta fidelidade, usar os mesmos tokens em Figma/Code; as imagens em `docs/ux/wireframes/` (quando geradas) servem como referência visual adicional.

— Uma (@ux-design-expert)
