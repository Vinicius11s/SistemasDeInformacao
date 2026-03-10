# Documento de Transição — Story 1.6: Códigos de Recuperação de Emergência (MFA Backup Codes)

**Gerado em:** 2026-03-09  
**Gerado por:** Morgan (@pm) + River (@sm) + @aios-master  
**Sprint:** 2 · **Story Points:** 8  
**Status ao encerrar sessão 1:** 🟡 Backend + Frontend completos — QA em andamento  
**Status ao encerrar sessão 2 (2026-03-10):** 🟢 QA concluído — pronto para `*push`

---

## 1. Ponto de Partida (Git)

```
Último commit registrado: 453a379  —  "Implementações camada segurança"
```

> **Nota para retomada:** Todo o trabalho da Story 1.6 está em **working directory modificado** (não commitado). Os arquivos novos aparecem como `??` e os modificados como `M` no `git status`. O primeiro ato ao retomar deve ser verificar `git diff --stat` para confirmar que nada foi perdido, e então fazer um commit de ponto de controle antes de continuar.

---

## 2. Erros Solucionados nesta Sessão

### 2.1 Schema Drift — Colunas `foto_url` e `oab` ausentes

**Sintoma:** `PostgresException (42703): column "foto_url" does not exist` ao carregar perfil do advogado.

**Causa-raiz:** A entidade `Advogado` tinha as propriedades `FotoUrl` e `OAB` definidas em C#, mas a `AdvogadoConfiguration` não mapeava explicitamente os nomes snake_case das colunas. O EF Core gerava SQL com os nomes Pascal (`"FotoUrl"`, `"OAB"`) que o PostgreSQL não reconhecia.

**Correção aplicada** em `AdvogadoConfiguration.cs`:
```csharp
builder.Property(e => e.FotoUrl).HasColumnName("foto_url");
builder.Property(e => e.OAB).HasColumnName("oab").HasMaxLength(20);
// + mapeamentos explícitos para todos os demais campos snake_case
```
Uma migration `20260309120000_AddMfaColumnsToAdvogado` foi criada para adicionar as colunas ausentes (mfa_enabled, mfa_secret, foto_url, role, nome_escritorio, etc.).

---

### 2.2 `InvalidCastException` — `DateTimeOffset` vs. `date` (coluna `data_expiracao`)

**Sintoma:** `System.InvalidCastException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'date'` ao persistir ou ler o campo `DataExpiracao` do advogado.

**Causa-raiz:** A propriedade `DataExpiracao` era `DateTimeOffset?` no C#, mas a coluna no banco é do tipo `date` (sem timezone). O Npgsql rejeita a conversão implícita.

**Correção aplicada:**
- Tipo C# alterado de `DateTimeOffset?` → **`DateOnly?`** em `Advogado.cs`
- DTO `AdvogadoProfileResponse` atualizado de `DateTimeOffset?` → `DateOnly?`
- Configuração Fluent API: `.HasColumnType("date").HasColumnName("data_expiracao")`
- Migration atualizada para usar `type: "date"` no `AddColumn`

---

## 3. Progresso da Story 1.6

### 3.1 Backend — ✅ B1–B11 + B7a (100% implementado)

| Task | Arquivo | Status |
|------|---------|--------|
| B1 — BCrypt.Net-Next | `Agile360.Infrastructure.csproj` | ✅ |
| B2 — Entidade `RecoveryCode` | `Domain/Entities/RecoveryCode.cs` | ✅ Novo |
| B3 — `RecoveryCodeConfiguration` | `Data/Configurations/RecoveryCodeConfiguration.cs` | ✅ Novo |
| B4 — `DbSet<RecoveryCode>` | `Agile360DbContext.cs` | ✅ |
| B5 — Migration `AddRecoveryCodesTable` | `20260309194614_AddRecoveryCodesTable.cs` | ✅ Novo |
| B6 — Interface `IRecoveryCodeService` | `Application/Interfaces/IRecoveryCodeService.cs` | ✅ Novo |
| B7 — Implementação `RecoveryCodeService` | `Infrastructure/Auth/RecoveryCodeService.cs` | ✅ Novo |
| B7a — Atomicidade (`ExecuteUpdateAsync WHERE is_used=false`) | `RecoveryCodeService.ValidateAndConsumeAsync` | ✅ |
| B8 — Registro no DI | `DependencyInjection.cs` | ✅ |
| B9 — `RecoveryCodesController` (3 endpoints) | `Controllers/RecoveryCodesController.cs` | ✅ Novo |
| B10 — `CompleteSetupAsync` → gera códigos | `MfaService.cs` | ✅ (lógica movida para controller) |
| B11 — `DisableAsync` → deleta códigos | `MfaService.cs` | ✅ (lógica movida para controller) |

**Detalhe de segurança — B7a (TRAVA DO PO — NÃO ALTERAR):**
```csharp
// ValidateAndConsumeAsync — proteção contra race condition
var rowsAffected = await _db.RecoveryCodes
    .Where(c => c.Id == matchedId.Value && !c.IsUsed)   // ← WHERE is_used = false
    .ExecuteUpdateAsync(s => s
        .SetProperty(c => c.IsUsed, true)
        .SetProperty(c => c.UsedAt, now), ct);

return rowsAffected == 1;  // ← verifica que apenas 1 linha foi afetada
```
> ⚠️ **Não remover o `&& !c.IsUsed` do filtro.** Ele é o único mecanismo que garante que duas requisições simultâneas com o mesmo código não autentiquem o mesmo usuário duas vezes (race condition).

---

### 3.2 Frontend — ✅ F1–F8 + F6a (100% implementado)

| Task | Arquivo | Status |
|------|---------|--------|
| F1 + F2 — Tipos e funções `api/mfa.ts` | `frontend/src/api/mfa.ts` | ✅ |
| F3 + F4 + F5 — Stepper 3 passos + view `setup-backup` | `SecuritySettings.tsx` | ✅ |
| F6 + F6a — Badge "X de 10 restantes" + alerta ≤ 2 | `SecuritySettings.tsx` | ✅ |
| F7 — Input de código de recuperação em `MfaChallenge` | `MfaChallenge.tsx` | ✅ |
| F8 — `completeMfaChallengeWithRecovery` no contexto | `AuthContext.tsx` | ✅ |

**Destaques da implementação frontend:**
- **QR Code local:** substituído `api.qrserver.com` (externo) por `QRCodeSVG` da lib `qrcode.react` — chave secreta nunca sai do navegador.
- **Stepper visual:** 3 passos `① Escanear QR Code → ② Confirmar código → ③ Backup de Emergência`
- **Grid de códigos:** 10 códigos exibidos em grid 2 colunas com botões "Baixar .txt" e "Copiar todos"
- **Checkbox obrigatório:** botão "Concluir" permanece desabilitado até checkbox marcado
- **Tabs no MfaChallenge:** aba `🔐 Autenticador` (TOTP 6 dígitos) + aba `🔑 Código de Emergência` (XXXX-XXXX)
- **Rate limit visual:** badge de alerta vermelho/amarelo quando `remaining ≤ 2`

---

## 4. Ponto de Retomada

### ✅ Próximo passo imediato: `*review-build` da @qa (Quinn)

**O que Quinn deve validar:**

1. **Rastreabilidade Frontend ↔ Backend — Rate Limit:**
   - Backend registra policy `"mfa-generate"` → `3 req / 1 hora` em `Program.cs`
   - Frontend exibe alerta quando `remaining ≤ 2` (badge em `SecuritySettings.tsx`)
   - Teste de integração `RecoveryCodesRateLimitTests` em `tests/Agile360.IntegrationTests/Api/`

2. **Botão "Concluir ativação" — dependência do checkbox:**
   - Em `SecuritySettings.tsx`, view `setup-backup`:
     ```tsx
     <Button disabled={!backupAcknowledged} onClick={() => setView('status')}>
       Concluir
     </Button>
     ```
   - O checkbox `backupAcknowledged` é a **única trava** antes de fechar o stepper. Confirmar que não há caminho para fechar sem marcar.

3. **Regressão — login TOTP normal não afetado:**
   - `RecoveryCodeService` agora aceita `Agile360DbContext` diretamente (não mais `DbContext` genérico)
   - Confirmar que o fluxo `POST /api/auth/mfa/challenge` (TOTP) ainda funciona sem chamar `IRecoveryCodeService`
   - `MfaController` e `RecoveryCodesController` são **controllers separados** — sem interferência

---

### ✅ Bloqueador de testes de integração — RESOLVIDO (2026-03-10)

Os 3 problemas identificados foram corrigidos por Quinn (@qa) na retomada:

| # | Problema | Causa-raiz | Status |
|---|----------|------------|--------|
| 1 | `No authentication handler for scheme 'Bearer'` | Factory registrava apenas `"Test"` | ✅ Corrigido — adicionado `"Bearer"` |
| 2 | `No authentication handler for scheme 'ApiKey'` (novo 500) | `DefaultPolicy` do `Program.cs` exigia scheme `"ApiKey"` que não existe nos testes | ✅ **Corrigido** — removido `IConfigureOptions<AuthorizationOptions>` e re-registrado policy com apenas `"Test"/"Bearer"` |
| 3 | `ValidateAndConsumeAsync_BurnAfterUse` falha no unit test | `ExecuteUpdateAsync` não propaga para o change tracker; query de assert sem `AsNoTracking()` retornava instância obsoleta | ✅ **Corrigido** — adicionado `.AsNoTracking()` na query de assert |

**Resultados finais dos testes:**
- **Integração:** `Aprovado: 4, Com falha: 0` — todos os testes Q4 (rate limit) passando.
- **Unitários Story 1.6:** `Aprovado: 19, Com falha: 0` — Q1–Q5 completamente aprovados.
- **Build:** `0 warnings, 0 errors` — solução limpa.

**Arquivos modificados nesta sessão (pré-push):**
```
tests/Agile360.IntegrationTests/Api/RecoveryCodesRateLimitTests.cs    (+41 / -23)
src/Agile360.Infrastructure/Data/Configurations/RecoveryCodeConfiguration.cs  (+8 / -3)
tests/Agile360.UnitTests/Infrastructure/Auth/RecoveryCodeServiceTests.cs       (+6 / -1)
```

---

## 5. Pendências Críticas — Travas de Segurança do @po (Pax)

> **ATENÇÃO:** Os itens abaixo foram explicitamente validados e aprovados por Pax durante a sprint. **Não devem ser alterados sem nova aprovação do PO.**

### 5.1 🔴 Race Condition — `ValidateAndConsumeAsync` (B7a)

**Regra:** O UPDATE no banco **deve** incluir `WHERE is_used = false` e verificar `rowsAffected == 1`.  
**Risco se removido:** Duas requisições simultâneas com o mesmo código passam pelo `BCrypt.Verify` em paralelo, ambas recebem `true`, e ambas autenticam o usuário — comprometendo o princípio de uso único (burn-after-use).

```csharp
// NÃO SIMPLIFICAR para .Where(c => c.Id == matchedId.Value)
// O && !c.IsUsed é a barreira atômica contra race condition
.Where(c => c.Id == matchedId.Value && !c.IsUsed)
```

### 5.2 🔴 Rate Limit — Endpoint `/generate` (3 req/hora)

**Regra:** O endpoint `POST /api/auth/mfa/recovery-codes/generate` **deve** manter `[EnableRateLimiting("mfa-generate")]` com `PermitLimit = 3` por janela de **1 hora**.  
**Risco se removido ou aumentado:** Cada chamada executa 10× BCrypt(cost 12) ≈ 2,5 segundos de CPU. Sem limite, um atacante autenticado pode gerar DoS da API via chamadas em loop.

```csharp
// Em Program.cs — NÃO alterar o PermitLimit nem o Window sem aprovação do PO
options.AddFixedWindowLimiter("mfa-generate", c => {
    c.Window      = TimeSpan.FromHours(1);
    c.PermitLimit = 3;
});
```

### 5.3 🟡 Código em texto limpo — exibição única

**Regra:** Os códigos plaintext **só podem ser retornados** pelo endpoint `generate` e devem ser exibidos **uma única vez** no frontend. O banco armazena apenas o hash BCrypt.  
**Risco se alterado:** Armazenar ou reexibir os códigos em sessão/localStorage os tornaria tão vulneráveis quanto senhas em texto limpo.

---

## 6. Arquivos Chave da Story 1.6

```
NOVOS (não commitados):
  src/Agile360.Domain/Entities/RecoveryCode.cs
  src/Agile360.Application/Interfaces/IRecoveryCodeService.cs
  src/Agile360.Infrastructure/Auth/RecoveryCodeService.cs
  src/Agile360.Infrastructure/Data/Configurations/RecoveryCodeConfiguration.cs
  src/Agile360.Infrastructure/Data/Migrations/20260309120000_AddMfaColumnsToAdvogado.cs
  src/Agile360.Infrastructure/Data/Migrations/20260309194614_AddRecoveryCodesTable.cs
  src/Agile360.Infrastructure/Data/Migrations/20260309194614_AddRecoveryCodesTable.Designer.cs
  src/Agile360.API/Controllers/RecoveryCodesController.cs
  tests/Agile360.IntegrationTests/Api/RecoveryCodesRateLimitTests.cs
  tests/Agile360.UnitTests/Infrastructure/  (diretório com RecoveryCodeServiceTests.cs)
  docs/sql/006_AddRecoveryCodesTable.sql

MODIFICADOS (não commitados):
  src/Agile360.Domain/Entities/Advogado.cs                      (DataExpiracao: DateTimeOffset→DateOnly)
  src/Agile360.Infrastructure/Data/Configurations/AdvogadoConfiguration.cs  (mapeamentos snake_case)
  src/Agile360.Infrastructure/Data/Agile360DbContext.cs          (DbSet<RecoveryCode>)
  src/Agile360.Infrastructure/Auth/MfaService.cs                (lógica recovery movida ao controller)
  src/Agile360.Infrastructure/DependencyInjection.cs            (registro IRecoveryCodeService)
  src/Agile360.API/Program.cs                                   (policy "mfa-generate")
  src/Agile360.Application/Auth/DTOs/MfaDtos.cs                 (MfaActivatedResponse com RecoveryCodes)
  src/Agile360.Application/Auth/DTOs/AdvogadoProfileResponse.cs (DataExpiracao: DateOnly?)
  frontend/src/api/mfa.ts                                       (novos tipos e funções)
  frontend/src/api/auth.ts                                      (SecureAuthResponse exportado)
  frontend/src/api/client.ts                                    (deleteWithBody)
  frontend/src/context/AuthContext.tsx                          (completeMfaChallengeWithRecovery)
  frontend/src/pages/SecuritySettings.tsx                       (stepper 3 passos + grid códigos)
  frontend/src/pages/MfaChallenge.tsx                           (tabs TOTP + recovery)
  frontend/package.json                                         (qrcode.react adicionado)
```

---

## 7. Comandos Úteis para Retomada

```bash
# Verificar estado do working directory
git diff --stat
git status --short

# Rodar todos os testes unitários
dotnet test tests/Agile360.UnitTests/Agile360.UnitTests.csproj

# Rodar testes de integração (ver o bloqueador do item 4)
dotnet test tests/Agile360.IntegrationTests/Agile360.IntegrationTests.csproj

# Diagnóstico do erro 500 pendente (próxima ação da @qa)
dotnet test tests/Agile360.IntegrationTests/Agile360.IntegrationTests.csproj --filter "Diagnostico_PrimeiraRequisicao" -v normal

# Aplicar migrations no banco de staging (após resolver testes)
dotnet ef database update --project src/Agile360.Infrastructure --startup-project src/Agile360.API
```

---

## 8. Definition of Done — Situação Atual

| Critério | Status |
|----------|--------|
| Migration aplicada em staging sem erros | 🔴 Pendente (aguarda resolução do bloqueador de testes) |
| 10 códigos gerados ao ativar MFA | ✅ Implementado |
| Passo 3 "Backup de Emergência" no stepper | ✅ Implementado |
| Botão "Baixar .txt" funcional | ✅ Implementado |
| Checkbox de confirmação bloqueia "Concluir" | ✅ Implementado |
| Login com código de recuperação válido | ✅ Implementado |
| Código usado retorna 401 na segunda tentativa | ✅ Implementado |
| 4ª tentativa inválida retorna 429 | ✅ Teste de integração aprovado (Q4) |
| Desativar MFA deleta todos os códigos | ✅ Implementado |
| Testes unitários Q1–Q5 | ✅ 19/19 aprovados (sessão 2026-03-10) |
| Nenhum código plaintext no banco | ✅ Garantido por design (só BCrypt hash) |
| Endpoint `generate` retorna 400 se MFA inativo | ✅ Implementado |
| Alerta visual quando `remaining ≤ 2` | ✅ Implementado |
| `ValidateAndConsumeAsync` protegido contra race condition | ✅ Implementado (B7a) |

---

*Documento gerado automaticamente por @aios-master · Story 1.6 · Sprint 2 · 2026-03-09*
