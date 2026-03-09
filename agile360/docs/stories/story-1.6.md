# Story 1.6: Códigos de Recuperação de Emergência (MFA Backup Codes)

**Epic:** Fundação e Infraestrutura — Segurança & Autenticação  
**Story ID:** 1.6  
**Sprint:** 2  
**Priority:** 🔴 Critical  
**Points:** 8  
**Effort:** 6–9 horas  
**Status:** ✅ Ready for Dev  
**Type:** 🔒 Security  

> **Depende de:** Story 1.3 (Auth Local) · MFA TOTP implementado (sprint 1)  
> **Criado por:** River (@sm) · **Data:** 2026-03-09  

---

## 🔀 Cross-Story Decisions

| Decisão | Fonte | Impacto nesta Story |
|---------|-------|---------------------|
| MFA via Google Authenticator já implementado | Sprint 1 | Setup flow existente (SecuritySettings.tsx) deve ganhar Passo 3 — "Backup de Emergência" |
| Secrets TOTP nunca em texto limpo (AES-256-GCM) | Story MFA | Mesma política se aplica aos códigos de recuperação: armazenar apenas o hash |
| EF Core + Npgsql + Supabase | Story 1.1 | Nova tabela `advogado_recovery_codes` via migration EF Core |
| `DateOnly` para datas sem timezone | Correção sprint atual | `CreatedAt` nos recovery codes usa `DateTimeOffset` (auditoria completa) |
| Rate limiting via `EnableRateLimiting("auth-login")` | Story 1.3 | Endpoint de uso de código de recuperação herda o mesmo policy |

---

## 📋 User Story

**Como** advogado que usa autenticação em duas etapas no Agile360,  
**Quero** receber 10 códigos de recuperação de emergência no momento em que ativo o 2FA,  
**Para** conseguir acessar minha conta mesmo que eu perca o celular ou o Google Authenticator seja desinstalado — sem precisar de suporte humano.

---

## 🎯 Objetivo

Implementar o fluxo completo de **Backup Codes** para MFA:

1. **Geração** de 10 códigos alfanuméricos únicos (8 caracteres) no momento do setup do TOTP.  
2. **Armazenamento seguro** — somente o hash (BCrypt) de cada código é persistido; o texto limpo é exibido apenas uma vez.  
3. **UX de Backup** — nova etapa no stepper de segurança com download em `.txt` e checkbox de confirmação.  
4. **Login de emergência** — o campo de TOTP em `MfaChallenge` aceita um código de recuperação como alternativa; o código é invalidado após uso (burn-after-use).  
5. **Bloqueio anti-brute-force** — 3 tentativas consecutivas inválidas bloqueiam o fluxo por 15 minutos.

---

## 🏗️ @architect — Requisitos Técnicos

### Geração dos Códigos

- **Quantidade:** 10 códigos por ativação de MFA.  
- **Formato:** `XXXX-XXXX` — 8 caracteres alfanuméricos (A–Z, 0–9, sem ambíguos: `0/O`, `1/I/L`), com hífen visual no meio.  
- **Entropia:** gerados via `RandomNumberGenerator.GetBytes` (.NET) — mínimo 40 bits de entropia por código.  
- **Exibição:** texto limpo **uma única vez**, no momento da geração, no frontend. Após fechar o modal, irreproduziveis.  
- **Regeneração:** ao re-ativar o MFA ou via botão "Gerar novos códigos" (invalida todos os anteriores).

### Regras de Negócio

| Regra | Detalhe |
|-------|---------|
| Burn-after-use | Um código usado com sucesso tem `IsUsed = true`; nunca é aceito novamente |
| Expiração | Códigos não expiram por tempo — apenas por uso ou regeneração |
| Regeneração | Gerar novos códigos invalida (`IsUsed = true`) todos os anteriores do advogado |
| Limite de uso no login | Máximo 1 código de recuperação por sessão de challenge |
| Bloqueio anti-brute-force | 3 tentativas inválidas consecutivas → bloqueio de 15 min (mesmo policy do login) |
| MFA desativado | Ao desativar o MFA, todos os códigos são deletados (hard delete) |

---

## 🗄️ @data-engineer — Esquema de Banco de Dados

### Nova Tabela: `advogado_recovery_codes`

```sql
-- Migration EF Core: AddRecoveryCodesTables
CREATE TABLE public.advogado_recovery_codes (
    id              uuid                     PRIMARY KEY DEFAULT gen_random_uuid(),
    advogado_id     uuid                     NOT NULL
                                             REFERENCES public.advogado(id) ON DELETE CASCADE,
    code_hash       character varying(100)   NOT NULL,   -- BCrypt hash do código limpo
    is_used         boolean                  NOT NULL DEFAULT false,
    used_at         timestamptz,                         -- NULL enquanto não usado
    created_at      timestamptz              NOT NULL DEFAULT now()
);

-- Índice para busca rápida dos códigos ativos de um advogado
CREATE INDEX ix_recovery_codes_advogado_active
    ON public.advogado_recovery_codes (advogado_id)
    WHERE is_used = false;
```

### Política de Segurança dos Dados

| Campo | Armazenamento | Observação |
|-------|--------------|------------|
| `code_hash` | BCrypt (cost factor 12) | Nunca o código em texto limpo |
| `is_used` | boolean | Auditado com `used_at` |
| `advogado_id` | FK com CASCADE DELETE | Limpeza automática ao deletar advogado |

> **Nota:** Usar **BCrypt** (não Argon2) para compatibilidade imediata com `BCrypt.Net-Next` já disponível no ecossistema .NET. Cost factor 12 é adequado para códigos de 8 chars. Argon2 pode ser considerado em story futura de hardening.

### Entidade EF Core

```csharp
// Domain/Entities/RecoveryCode.cs
public class RecoveryCode
{
    public Guid            Id          { get; set; }
    public Guid            AdvogadoId  { get; set; }
    public string          CodeHash    { get; set; } = string.Empty;
    public bool            IsUsed      { get; set; } = false;
    public DateTimeOffset? UsedAt      { get; set; }
    public DateTimeOffset  CreatedAt   { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Advogado? Advogado { get; set; }
}
```

### Configuração Fluent API

```csharp
// Infrastructure/Data/Configurations/RecoveryCodeConfiguration.cs
builder.ToTable("advogado_recovery_codes");
builder.HasKey(e => e.Id);
builder.Property(e => e.CodeHash).HasMaxLength(100).IsRequired();
builder.Property(e => e.IsUsed).HasDefaultValue(false);
builder.Property(e => e.UsedAt).HasColumnType("timestamp with time zone");
builder.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
builder.HasIndex(e => new { e.AdvogadoId, e.IsUsed });
builder.HasOne(e => e.Advogado)
       .WithMany()
       .HasForeignKey(e => e.AdvogadoId)
       .OnDelete(DeleteBehavior.Cascade);
```

---

## 🎨 @ux-design-expert — Novo Passo 3 no Stepper de Segurança

O stepper existente em `SecuritySettings.tsx` passa de **2 para 3 passos**:

```
① Escanear QR Code  ──────  ② Confirmar código  ──────  ③ Backup de Emergência
```

### Wireframe — Passo 3: "Backup de Emergência"

```
┌─────────────────────────────────────────────────────────────────┐
│  ① Escanear QR Code ─────── ② Confirmar código ─────── ③ Backup │
│                                                                   │
│  🔑  Seus Códigos de Recuperação de Emergência                   │
│  ─────────────────────────────────────────────────────────────  │
│  Guarde estes códigos em um local seguro. Cada código pode ser   │
│  usado UMA VEZ para acessar sua conta se perder o celular.       │
│                                                                   │
│  ┌─────────────────────────────────────────────────────┐         │
│  │  A3KW-X9PL    B7NR-M2QT    C5VH-J4YS    D8FG-K1ZE  │         │
│  │  E2XP-R6MN    F9YT-W3LC    G4BH-Q7DU    H1JV-N5RI  │         │
│  │  I6KM-S0FP    J3TW-A8GX                             │         │
│  └─────────────────────────────────────────────────────┘         │
│                                                                   │
│  [⬇ Baixar como .txt]    [📋 Copiar todos]                       │
│                                                                   │
│  ⚠️ Estes códigos NÃO serão exibidos novamente.                  │
│                                                                   │
│  ┌─────────────────────────────────────────────────────┐         │
│  │  ☐  Eu guardei meus códigos em um local seguro e    │         │
│  │     entendo que cada um pode ser usado apenas 1 vez │         │
│  └─────────────────────────────────────────────────────┘         │
│                                                                   │
│  [Concluir ativação do 2FA ✓]   ← desabilitado até ☑ checkbox   │
└─────────────────────────────────────────────────────────────────┘
```

### Comportamento do Botão "Baixar .txt"

Conteúdo do arquivo `agile360-codigos-recuperacao.txt`:

```
Agile360 — Códigos de Recuperação de Emergência
================================================
Gerados em: 09/03/2026 às 18:42 UTC
Conta: advogado@email.com

IMPORTANTE: Guarde este arquivo em local seguro.
Cada código pode ser usado UMA única vez.
Após o uso, o código é invalidado automaticamente.

A3KW-X9PL
B7NR-M2QT
C5VH-J4YS
D8FG-K1ZE
E2XP-R6MN
F9YT-W3LC
G4BH-Q7DU
H1JV-N5RI
I6KM-S0FP
J3TW-A8GX
```

---

## 💻 @dev — Especificação de Implementação

### Novos Endpoints

| Método | Rota | Auth | Rate Limit | Descrição |
|--------|------|------|------------|-----------|
| `POST` | `/api/auth/mfa/recovery-codes/generate` | JWT + **MFA ativo obrigatório** | **3 req / hora por advogado** | Gera (ou regenera) os 10 códigos; retorna plaintext |
| `GET` | `/api/auth/mfa/recovery-codes/count` | JWT | Sem limite | Retorna `{ remaining: N }` — quantos não usados restam |
| `POST` | `/api/auth/mfa/challenge/recovery` | Anônimo | `auth-login` (existente) | Valida código de recuperação em vez de TOTP |

> **Notas de segurança:**
> - O endpoint `generate` **exige que `MfaEnabled = true`** — impede geração de códigos para advogados sem MFA ativo (retorna `400` caso contrário).
> - O endpoint `generate` **sempre** invalida os códigos anteriores antes de criar novos.
> - Rate limit de `3 req/hora` no `generate` protege contra DoS por BCrypt (10 × ~250ms = 2.5s de CPU por chamada).
> - O endpoint `challenge/recovery` herda o policy `auth-login` já configurado.

### Alteração em `MfaChallenge.tsx`

O campo de código TOTP deve aceitar tanto 6 dígitos (TOTP) quanto 9 caracteres com hífen (código de recuperação). O frontend detecta o formato e chama o endpoint correto:

```
6 dígitos  → POST /api/auth/mfa/challenge          (TOTP)
9 chars    → POST /api/auth/mfa/challenge/recovery  (código de backup)
```

Adicionar abaixo do input um link discreto:  
`"Sem acesso ao app? Usar código de emergência →"` que expande um input separado.

### Interface `IRecoveryCodeService`

```csharp
public interface IRecoveryCodeService
{
    /// Gera 10 novos códigos, invalida os anteriores, persiste hashes.
    /// Retorna os códigos em plaintext (ÚNICA exibição).
    Task<IReadOnlyList<string>> GenerateCodesAsync(Guid advogadoId, CancellationToken ct = default);

    /// Valida um código de recuperação. Se válido → marca IsUsed = true.
    Task<bool> ValidateAndConsumeAsync(Guid advogadoId, string code, CancellationToken ct = default);

    /// Retorna quantos códigos não usados o advogado ainda possui.
    Task<int> GetRemainingCountAsync(Guid advogadoId, CancellationToken ct = default);

    /// Remove todos os códigos (chamado ao desativar MFA).
    Task DeleteAllAsync(Guid advogadoId, CancellationToken ct = default);
}
```

### Formato do Código

```csharp
private static string GenerateSingleCode()
{
    // Alfabet sem ambíguos: sem 0,O,1,I,L
    const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    var bytes = RandomNumberGenerator.GetBytes(8);
    var chars = bytes.Select(b => alphabet[b % alphabet.Length]).ToArray();
    // Formato visual: XXXX-XXXX
    return $"{new string(chars[..4])}-{new string(chars[4..])}";
}
```

### Pacote necessário

```
dotnet add package BCrypt.Net-Next
```

---

## ✅ @qa — Critérios de Aceitação (Gherkin)

```gherkin
# ── Geração ──────────────────────────────────────────────────────────────

Feature: Geração de Códigos de Recuperação

  Scenario: Geração de 10 códigos únicos no setup do MFA
    Given o advogado concluiu o passo 2 do setup (TOTP verificado)
    When o sistema exibe o passo 3 "Backup de Emergência"
    Then são exibidos exatamente 10 códigos no formato XXXX-XXXX
    And cada código tem 9 caracteres visíveis (8 alfanum + 1 hífen)
    And todos os 10 códigos são diferentes entre si
    And o banco contém 10 registros com is_used = false para o advogado

  Scenario: Códigos não são exibidos novamente após fechar
    Given os códigos foram exibidos no passo 3
    When o advogado fecha o modal e acessa novamente as configurações
    Then a UI exibe apenas "X de 10 códigos restantes"
    And os códigos em texto limpo NÃO são exibidos

  Scenario: Regeneração invalida códigos anteriores
    Given o advogado possui 7 códigos não usados
    When ele solicita "Gerar novos códigos"
    Then os 7 códigos anteriores têm is_used = true no banco
    And 10 novos códigos são gerados e exibidos

# ── Uso no Login ──────────────────────────────────────────────────────────

Feature: Login com Código de Recuperação

  Scenario: Login bem-sucedido com código válido
    Given o advogado tem MFA ativo e possui 5 códigos não usados
    And ele está na tela MfaChallenge
    When ele insere um código de recuperação válido (ex.: "A3KW-X9PL")
    Then o sistema autentica e redireciona para /app
    And o código usado tem is_used = true e used_at preenchido no banco
    And o advogado possui 4 códigos não usados restantes

  Scenario: Código de recuperação já usado não funciona
    Given o código "A3KW-X9PL" já foi usado (is_used = true)
    When o advogado tenta usar "A3KW-X9PL" no login
    Then o sistema retorna 401 "Código de recuperação inválido ou já utilizado."
    And o código NÃO é marcado novamente (permanece is_used = true)

  Scenario: Código inexistente retorna 401 sem diferenciação de erro
    Given o código "ZZZZ-9999" não existe para nenhum advogado
    When é submetido no endpoint POST /api/auth/mfa/challenge/recovery
    Then o sistema retorna 401 com mesma mensagem genérica
    And o tempo de resposta é similar ao de um código inválido real (sem timing attack)

# ── Bloqueio Anti-Brute-Force ─────────────────────────────────────────────

Feature: Bloqueio após Tentativas Inválidas

  Scenario: Bloqueio após 3 tentativas inválidas consecutivas
    Given o advogado está na tela de challenge do MFA
    When ele submete 3 códigos de recuperação inválidos consecutivos
    Then na 4ª tentativa o sistema retorna 429 Too Many Requests
    And a mensagem é "Muitas tentativas inválidas. Tente novamente em 15 minutos."
    And o bloqueio dura exatamente 15 minutos

  Scenario: Autenticação TOTP bem-sucedida reseta o contador de tentativas de recuperação
    # Razão: após identidade comprovada via TOTP, o bloqueio de recovery é levantado
    # para que o próprio advogado possa tentar seus códigos de backup novamente.
    Given o advogado fez 2 tentativas inválidas de código de recuperação
    And ainda não foi bloqueado (faltam 1 tentativa para o bloqueio)
    When ele submete um código TOTP válido e autentica com sucesso
    Then o contador de tentativas de códigos de recuperação é zerado para 0
    And numa sessão futura ele pode tentar até 3 vezes novamente antes do bloqueio

  Scenario: Rate limit de recovery é independente do rate limit de TOTP
    # Os dois contadores são isolados — esgotar recovery não bloqueia TOTP.
    Given o advogado atingiu o bloqueio de recovery (3 tentativas inválidas)
    When ele submete um código TOTP válido na mesma sessão de challenge
    Then o TOTP é aceito normalmente (HTTP 200)
    And o advogado é autenticado com sucesso

# ── Códigos Esgotados ─────────────────────────────────────────────────────

Feature: Aviso e Bloqueio com Códigos Críticos ou Esgotados

  Scenario: UI exibe alerta quando restam 2 ou menos códigos
    Given o advogado usou 8 dos 10 códigos (restam 2)
    When ele acessa a tela de Segurança e Privacidade
    Then a badge de status exibe "⚠️ 2 de 10 códigos restantes"
    And uma mensagem de alerta sugere regenerar os códigos em breve

  Scenario: Tentativa de login com recovery quando não há códigos disponíveis
    Given o advogado usou todos os 10 códigos (remaining = 0)
    When ele tenta usar um código de recuperação no MfaChallenge
    Then o sistema retorna 401 com a mensagem
      "Nenhum código de recuperação disponível. Regenere seus códigos nas configurações de segurança."
    And o endpoint GET /api/auth/mfa/recovery-codes/count retorna { "remaining": 0 }

# ── Desativação do MFA ────────────────────────────────────────────────────

Feature: Limpeza de Códigos ao Desativar MFA

  Scenario: Desativar MFA remove todos os códigos
    Given o advogado tem 4 códigos de recuperação não usados
    When ele desativa o MFA com um código TOTP válido
    Then todos os registros em advogado_recovery_codes são deletados
    And GET /api/auth/mfa/recovery-codes/count retorna { "remaining": 0 }
```

---

## ✅ Tasks de Desenvolvimento

### Backend

- [ ] **B1** — Instalar `BCrypt.Net-Next` no projeto `Agile360.Infrastructure`
- [ ] **B2** — Criar entidade `RecoveryCode` em `Domain/Entities/`
- [ ] **B3** — Criar `RecoveryCodeConfiguration.cs` (Fluent API)
- [ ] **B4** — Adicionar `DbSet<RecoveryCode> RecoveryCodes` ao `Agile360DbContext`
- [ ] **B5** — Criar migration `AddRecoveryCodesTable`
- [ ] **B6** — Criar interface `IRecoveryCodeService` em `Application/Interfaces/`
- [ ] **B7** — Implementar `RecoveryCodeService` em `Infrastructure/Auth/`
- [ ] **B7a** — Garantir atomicidade em `ValidateAndConsumeAsync`: usar transação EF Core com `ExecuteUpdateAsync WHERE is_used = false` e verificar `rowsAffected == 1` — previne race condition em requisições concorrentes com o mesmo código
- [ ] **B8** — Registrar no DI em `DependencyInjection.cs`
- [ ] **B9** — Criar `RecoveryCodesController.cs` com os 3 endpoints
- [ ] **B10** — Alterar `MfaService.CompleteSetupAsync` para chamar `GenerateCodesAsync` após ativar MFA
- [ ] **B11** — Alterar `MfaService.DisableAsync` para chamar `DeleteAllAsync` após desativar MFA

### Frontend

- [ ] **F1** — Criar tipo `RecoveryCodesResponse` em `api/mfa.ts`
- [ ] **F2** — Adicionar funções `getRecoveryCodesCount` e `useRecoveryCode` em `api/mfa.ts`
- [ ] **F3** — Atualizar `SetupStepper` de 2 para 3 passos em `SecuritySettings.tsx`
- [ ] **F4** — Implementar view `'setup-backup'` com grid de códigos, botão download .txt e checkbox
- [ ] **F5** — Conectar `handleVerifySetup` para avançar para `'setup-backup'` em vez de `'status'`
- [ ] **F6** — Exibir badge "X de 10 códigos restantes" na view `status`
- [ ] **F6a** — Exibir alerta amarelo/vermelho quando `remaining ≤ 2`: "Atenção: restam apenas X códigos de recuperação. Gere novos em breve."
- [ ] **F7** — Atualizar `MfaChallenge.tsx` com input secundário para código de recuperação
- [ ] **F8** — Implementar `completeMfaChallengeWithRecovery` no `AuthContext`

### QA / Testes

- [ ] **Q1** — Testes unitários em `RecoveryCodeService`: geração, consumo, invalidação
- [ ] **Q2** — Teste de unicidade: 10 códigos gerados sempre distintos (rodar 1000x)
- [ ] **Q3** — Teste de burn-after-use: código consumido retorna `false` na segunda chamada
- [ ] **Q4** — Teste de rate limit: 4ª tentativa retorna 429
- [ ] **Q5** — Teste de timing: diferença entre código inválido e inexistente < 5ms

---

## 🗂️ Key Files (Novos e Modificados)

```
src/
├── Agile360.Domain/Entities/
│   └── RecoveryCode.cs                              [NOVO]
├── Agile360.Application/Interfaces/
│   └── IRecoveryCodeService.cs                      [NOVO]
├── Agile360.Infrastructure/
│   ├── Auth/
│   │   └── RecoveryCodeService.cs                   [NOVO]
│   └── Data/
│       ├── Configurations/
│       │   └── RecoveryCodeConfiguration.cs         [NOVO]
│       └── Migrations/
│           └── YYYYMMDDHHMMSS_AddRecoveryCodesTable.cs [NOVO]
├── Agile360.API/Controllers/
│   └── RecoveryCodesController.cs                   [NOVO]

frontend/src/
├── api/mfa.ts                                       [MODIFICADO]
├── context/AuthContext.tsx                          [MODIFICADO]
└── pages/
    ├── SecuritySettings.tsx                         [MODIFICADO — +passo 3]
    └── MfaChallenge.tsx                             [MODIFICADO — +input recovery]
```

---

## 🔗 Dependencies

**Blocked by:**
- MFA TOTP implementado (sprint 1) ✅
- `advogado` table com colunas `mfa_enabled`, `mfa_secret` ✅
- Rate limiting configurado em `Program.cs` ✅

**Blocks:**
- Story futura: "Notificação por e-mail ao usar código de recuperação"
- Story futura: "Audit log de eventos de segurança"

---

## ⚠️ Risks & Mitigations

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| Timing attack no `ValidateAndConsumeAsync` | Alto | Usar `BCrypt.Verify` que é constant-time; não retornar erro diferenciado |
| **Race condition em `ValidateAndConsumeAsync`** | **Alto** | **Duas requisições simultâneas com o mesmo código podem passar pelo BCrypt antes de uma marcar `is_used = true`. Mitigação: executar UPDATE com `WHERE is_used = false` em transação; verificar rowsAffected > 0 antes de prosseguir. Ver task B7a.** |
| Usuário não salva os códigos | Médio | Checkbox obrigatório + download .txt + aviso visual em vermelho |
| Regeneração acidental apaga códigos válidos | Médio | Modal de confirmação: "Isso invalidará seus X códigos atuais" |
| BCrypt lento (DoS por geração em loop) | Baixo | Rate limit no endpoint `generate`; cost factor 12 = ~250ms, aceitável |
| Códigos guardados em plaintext pelo usuário | Baixo | Responsabilidade do usuário; documentar boas práticas no .txt |

---

## 📋 Definition of Done

- [ ] Migration aplicada em banco de staging sem erros
- [ ] 10 códigos gerados automaticamente ao ativar MFA (fluxo `CompleteSetupAsync`)
- [ ] Passo 3 "Backup de Emergência" visível no stepper de `SecuritySettings`
- [ ] Botão "Baixar .txt" funcional com conteúdo correto
- [ ] Checkbox de confirmação bloqueia o botão "Concluir" até ser marcado
- [ ] Login com código de recuperação válido autentica o advogado
- [ ] Código usado retorna 401 na segunda tentativa
- [ ] 4ª tentativa inválida retorna 429 por 15 minutos
- [ ] Desativar MFA deleta todos os códigos do advogado
- [ ] Todos os critérios de aceitação (Gherkin) verificados e passando
- [ ] Testes unitários cobrindo os 5 cenários críticos (Q1–Q5)
- [ ] Nenhum código em texto limpo persiste no banco (auditoria pós-migration)
- [ ] Endpoint `generate` retorna `400` se `MfaEnabled = false` (não permite geração sem MFA ativo)
- [ ] Alerta visual exibido quando `remaining ≤ 2` na view de status de segurança
- [ ] `ValidateAndConsumeAsync` protegido contra race condition (transação com `WHERE is_used = false`)

---

## 📜 Change Log

| Data | Versão | Alterações | Autor |
|------|--------|-----------|-------|
| 2026-03-09 | 1.0.0 | Draft inicial — Códigos de Recuperação MFA | River (@sm) |
| 2026-03-09 | 1.1.0 | Revisão PO (Pax): corrigida contradição lógica no Gherkin (cenário "NÃO são resetadas" → "Autenticação TOTP reseta contador"); adicionados cenários de 0 códigos restantes e alerta ≤ 2; adicionado cenário de independência de rate limits; incluída mitigação de race condition em `ValidateAndConsumeAsync` (task B7a); especificado rate limit 3 req/h no endpoint `/generate`; adicionada guarda MFA ativo no `generate`; tasks F6a e itens de DoD adicionados. Status alterado para **Ready for Dev**. | Pax (@po) |

---

**Próximo passo:** Revisão do @po para priorização no Sprint 2.  
**Estimativa:** 8 story points · 6–9 horas de desenvolvimento + 2 horas QA.
