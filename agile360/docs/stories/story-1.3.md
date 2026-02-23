# Story 1.3: Authentication & Authorization (JWT + Supabase Auth)

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.3
**Sprint:** 1
**Priority:** 🔴 Critical
**Points:** 8
**Effort:** 6-10 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| Supabase Auth como provider | PRD Agile360 | Usar Supabase para gerenciar usuários e emitir JWT |
| Multi-Tenancy por advogado_id | Story 1.2 | JWT deve conter claim `advogado_id` para tenant resolution |
| .NET 9 Web API | Story 1.1 | Usar middleware nativo de autenticação do ASP.NET Core |

---

## 📋 User Story

**Como** advogado do Agile360,
**Quero** me autenticar de forma segura no sistema com email e senha,
**Para** ter acesso exclusivo e protegido aos meus dados jurídicos e de clientes.

---

## 🎯 Objective

Implementar o sistema de autenticação e autorização do Agile360 utilizando Supabase Auth para gerenciamento de identidade e JWT para autenticação na API .NET 9. Configurar o fluxo completo de registro, login, refresh token e proteção de endpoints, integrando com o Multi-Tenancy da Story 1.2.

---

## ✅ Tasks

### Phase 1: Supabase Auth Configuration (~2h)

- [ ] **1.1** Configurar Supabase Auth no projeto Supabase:
  - Habilitar Email/Password provider
  - Configurar redirect URLs
  - Definir password policy (mínimo 8 chars, complexidade)
  - Configurar e-mail templates (confirmação, reset)
- [ ] **1.2** Criar trigger no Supabase para vincular auth user → advogado:
  ```sql
  CREATE OR REPLACE FUNCTION public.handle_new_user()
  RETURNS trigger AS $$
  BEGIN
    INSERT INTO public.advogados (id, email, nome, created_at)
    VALUES (
      NEW.id,
      NEW.email,
      COALESCE(NEW.raw_user_meta_data->>'nome', 'Advogado'),
      NOW()
    );
    RETURN NEW;
  END;
  $$ LANGUAGE plpgsql SECURITY DEFINER;
  
  CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();
  ```
- [ ] **1.3** Configurar JWT secret do Supabase na API .NET:
  - Recuperar JWT secret do Supabase Dashboard
  - Armazenar em `appsettings` / environment variables
- [ ] **1.4** Criar custom claims no JWT do Supabase:
  ```sql
  CREATE OR REPLACE FUNCTION public.custom_access_token_hook(event jsonb)
  RETURNS jsonb AS $$
  DECLARE
    advogado_record RECORD;
  BEGIN
    SELECT id, nome, oab INTO advogado_record
    FROM public.advogados
    WHERE id = (event->>'user_id')::uuid;
    
    event := jsonb_set(event, '{claims,advogado_id}', to_jsonb(advogado_record.id));
    event := jsonb_set(event, '{claims,advogado_nome}', to_jsonb(advogado_record.nome));
    RETURN event;
  END;
  $$ LANGUAGE plpgsql;
  ```

### Phase 2: .NET JWT Authentication (~2h)

- [ ] **2.1** Instalar pacotes NuGet:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] **2.2** Configurar JWT Authentication em `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
          options.TokenValidationParameters = new TokenValidationParameters {
              ValidateIssuer = true,
              ValidIssuer = supabaseUrl,
              ValidateAudience = true,
              ValidAudience = "authenticated",
              ValidateIssuerSigningKey = true,
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(supabaseJwtSecret)),
              ValidateLifetime = true,
              ClockSkew = TimeSpan.Zero
          };
      });
  ```
- [ ] **2.3** Criar `JwtSettings` configuration class:
  ```json
  {
    "JwtSettings": {
      "Issuer": "https://<project>.supabase.co/auth/v1",
      "Audience": "authenticated",
      "Secret": "<supabase-jwt-secret>"
    }
  }
  ```
- [ ] **2.4** Configurar Authorization policies:
  - `RequireAuthenticated` – qualquer advogado autenticado
  - `RequireActiveAdvogado` – advogado com conta ativa
- [ ] **2.5** Criar `CurrentUserService` para extrair dados do JWT:
  ```csharp
  public interface ICurrentUserService {
      Guid AdvogadoId { get; }
      string Email { get; }
      string Nome { get; }
      bool IsAuthenticated { get; }
  }
  ```

### Phase 3: Auth Endpoints (~2h)

- [ ] **3.1** Criar `AuthController` com endpoints:
  - `POST /api/auth/register` – Registro de novo advogado
  - `POST /api/auth/login` – Login com email/senha
  - `POST /api/auth/refresh` – Refresh do access token
  - `POST /api/auth/logout` – Logout (invalidar session)
  - `POST /api/auth/forgot-password` – Solicitar reset de senha
  - `POST /api/auth/reset-password` – Reset com token
  - `GET /api/auth/me` – Dados do advogado logado
- [ ] **3.2** Criar DTOs:
  - `RegisterRequest` (nome, email, password, oab, telefone)
  - `LoginRequest` (email, password)
  - `AuthResponse` (accessToken, refreshToken, expiresAt, advogado)
  - `AdvogadoProfileResponse` (id, nome, email, oab, telefone, fotoUrl)
- [ ] **3.3** Criar `IAuthService` e `AuthService`:
  - Comunicação com Supabase Auth REST API
  - `RegisterAsync(RegisterRequest)`
  - `LoginAsync(LoginRequest)`
  - `RefreshTokenAsync(string refreshToken)`
  - `LogoutAsync(string accessToken)`
  - `ForgotPasswordAsync(string email)`
  - `ResetPasswordAsync(string token, string newPassword)`
- [ ] **3.4** Criar `SupabaseAuthClient` para chamadas HTTP:
  - Base URL: `https://<project>.supabase.co/auth/v1`
  - Headers: `apikey`, `Content-Type: application/json`
  - Retry policy com Polly
- [ ] **3.5** Criar validators com FluentValidation:
  - `RegisterRequestValidator` (email válido, senha forte, OAB formato)
  - `LoginRequestValidator` (campos obrigatórios)

### Phase 4: Proteção de Endpoints (~1h)

- [ ] **4.1** Aplicar `[Authorize]` em todos os controllers (exceto Auth)
- [ ] **4.2** Integrar `ICurrentUserService` com `ITenantProvider`:
  - `TenantMiddleware` usa `ICurrentUserService.AdvogadoId`
- [ ] **4.3** Criar `[AllowAnonymous]` para endpoints públicos:
  - Health check
  - Auth endpoints (register, login, forgot-password)
- [ ] **4.4** Criar testes de autorização:
  - Request sem token → 401 Unauthorized
  - Request com token inválido → 401 Unauthorized
  - Request com token expirado → 401 Unauthorized
  - Request com token válido → 200 OK

### Phase 5: Rate Limiting & Security (~1h)

- [ ] **5.1** Configurar Rate Limiting para auth endpoints:
  - Login: 5 tentativas por minuto por IP
  - Register: 3 tentativas por hora por IP
  - Forgot Password: 3 tentativas por hora por email
- [ ] **5.2** Adicionar HTTPS enforcement
- [ ] **5.3** Configurar security headers:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Strict-Transport-Security`
- [ ] **5.4** Criar log de audit para eventos de autenticação:
  - Login success/failure
  - Register
  - Password reset

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN o sistema de autenticação está configurado
WHEN um novo advogado se registra com email, senha, nome e OAB
THEN uma conta é criada no Supabase Auth
AND um registro de Advogado é criado na tabela advogados
AND um JWT é retornado com claim advogado_id

GIVEN um advogado registrado
WHEN faz login com email e senha corretos
THEN recebe um access_token JWT válido
AND recebe um refresh_token
AND o response contém dados do perfil do advogado

GIVEN um JWT válido
WHEN uma requisição autenticada é feita para qualquer endpoint protegido
THEN a requisição é processada com sucesso
AND o tenant é automaticamente resolvido via advogado_id do JWT

GIVEN nenhum JWT ou JWT inválido
WHEN uma requisição é feita para um endpoint protegido
THEN retorna 401 Unauthorized
AND o body contém mensagem de erro descritiva

GIVEN o rate limiting está configurado
WHEN mais de 5 tentativas de login falham no mesmo minuto
THEN retorna 429 Too Many Requests
AND o retry-after header indica o tempo de espera
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Autenticação é infraestrutura de segurança |
| Complexity | Medium | Integração com Supabase Auth, JWT validation |
| Test Requirements | Integration + Unit | Auth flows, token validation, rate limiting |
| Review Focus | Security | Autenticação é superfície crítica de ataque |

### Agent Assignment

| Role | Agent | Responsibility |
|------|-------|----------------|
| Primary | @dev | Implementação do auth flow |
| Secondary | @architect | Validação da integração Supabase Auth |
| Review | @qa | Testes de segurança, edge cases |

### Self-Healing Config

```yaml
reviews:
  auto_review:
    enabled: true
    drafts: false
  path_instructions:
    - path: "src/Agile360.API/Controllers/AuthController.cs"
      instructions: "Verificar que register/login seguem OWASP best practices"
    - path: "src/Agile360.Infrastructure/Auth/**"
      instructions: "Verificar token handling seguro, sem logging de secrets"
    - path: "sql/**"
      instructions: "Verificar SQL injection protection nas functions"

chat:
  auto_reply: true
```

### Focus Areas

- [ ] JWT Validation: Token validation rigorosa (issuer, audience, expiration)
- [ ] Password Security: Hashing seguro (bcrypt via Supabase), policy forte
- [ ] Rate Limiting: Proteção contra brute force em auth endpoints
- [ ] Secret Management: Nenhuma secret hardcoded, usar env vars
- [ ] Error Messages: Mensagens genéricas (não revelar se email existe)

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (API base, middleware pipeline)
- Story 1.2: Multi-Tenancy (Advogado entity, ITenantProvider)

**Blocks:**
- Story 2.1-2.4: CRM CRUD (endpoints autenticados)
- Story 3.1: WhatsApp Integration (webhook auth)
- Story 6.1-6.5: Dashboard (user context)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Supabase JWT secret rotation | High | Implementar key rotation graceful, cache invalidation |
| Token leak via logs | Critical | NUNCA logar tokens, mask em exception handler |
| Brute force em login | High | Rate limiting + account lockout temporário |
| CORS misconfiguration | Medium | Whitelist explícita de origens permitidas |
| Custom claims hook falha | Medium | Fallback para user_id como advogado_id |

---

## 📋 Definition of Done

- [ ] Supabase Auth configurado com Email/Password
- [ ] Trigger de criação de Advogado no registro
- [ ] JWT Authentication configurado na API .NET
- [ ] 7 endpoints de Auth implementados e funcionando
- [ ] CurrentUserService extraindo dados do JWT
- [ ] Integração TenantProvider ↔ CurrentUserService
- [ ] Rate limiting em auth endpoints
- [ ] Security headers configurados
- [ ] Audit log de eventos de autenticação
- [ ] Testes de autorização (401, 403, 200)
- [ ] All acceptance criteria verified
- [ ] Tests passing
- [ ] Documentation updated

---

## 📝 Dev Notes

### Key Files

```
src/
├── Agile360.API/
│   ├── Controllers/
│   │   └── AuthController.cs
│   ├── Middleware/
│   │   └── TenantMiddleware.cs (update from 1.2)
│   └── Program.cs (update - add JWT auth)
│
├── Agile360.Application/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── LoginCommand.cs
│   │   │   └── RefreshTokenCommand.cs
│   │   ├── Queries/
│   │   │   └── GetCurrentUserQuery.cs
│   │   ├── DTOs/
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── AuthResponse.cs
│   │   │   └── AdvogadoProfileResponse.cs
│   │   └── Validators/
│   │       ├── RegisterRequestValidator.cs
│   │       └── LoginRequestValidator.cs
│   └── Interfaces/
│       ├── IAuthService.cs
│       └── ICurrentUserService.cs
│
├── Agile360.Infrastructure/
│   ├── Auth/
│   │   ├── AuthService.cs
│   │   ├── CurrentUserService.cs
│   │   └── SupabaseAuthClient.cs
│   └── Configuration/
│       └── JwtSettings.cs
│
└── Agile360.Domain/
    └── Entities/
        └── Advogado.cs (from 1.2)

sql/
├── auth-trigger-new-user.sql
├── custom-claims-hook.sql
└── rls-policies.sql (update from 1.2)
```

### Supabase Auth REST API Reference

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/auth/v1/signup` | POST | Register |
| `/auth/v1/token?grant_type=password` | POST | Login |
| `/auth/v1/token?grant_type=refresh_token` | POST | Refresh |
| `/auth/v1/logout` | POST | Logout |
| `/auth/v1/recover` | POST | Forgot password |
| `/auth/v1/user` | PUT | Update user/password |
| `/auth/v1/user` | GET | Get user profile |

### Testing Checklist

#### Authentication Flow
- [ ] Register → cria user no Supabase + Advogado na tabela
- [ ] Login → retorna JWT válido com claims corretos
- [ ] Refresh → renova access token com refresh token válido
- [ ] Logout → invalida a sessão
- [ ] Forgot Password → envia email de reset

#### Authorization
- [ ] Endpoint protegido sem token → 401
- [ ] Endpoint protegido com token inválido → 401
- [ ] Endpoint protegido com token expirado → 401
- [ ] Endpoint protegido com token válido → 200
- [ ] `/api/auth/me` retorna perfil do advogado logado

#### Security
- [ ] Rate limiting funciona em login
- [ ] Passwords com menos de 8 chars rejeitados
- [ ] SQL injection nos SQL triggers impossível
- [ ] Tokens não aparecem em logs
- [ ] CORS rejeita origens não autorizadas

---

## 🧑‍💻 Dev Agent Record

> This section is populated when @dev executes the story.

### Execution Log

| Timestamp | Phase | Action | Result |
|-----------|-------|--------|--------|
| - | - | Awaiting execution | - |

### Implementation Notes

_To be filled during execution._

### Issues Encountered

_None yet - story not started._

---

## 🧪 QA Results

> This section is populated after @qa reviews the implementation.

### Test Execution Summary

| Category | Tests | Passed | Failed | Skipped |
|----------|-------|--------|--------|---------|
| Unit | - | - | - | - |
| Integration | - | - | - | - |
| E2E | - | - | - | - |

### Validation Checklist

| Check | Status | Notes |
|-------|--------|-------|
| Acceptance criteria | ⏳ | |
| DoD items | ⏳ | |
| Edge cases | ⏳ | |
| Documentation | ⏳ | |

### QA Sign-off

- [ ] All acceptance criteria verified
- [ ] Tests passing (coverage ≥80%)
- [ ] Documentation complete
- [ ] Ready for release

**QA Agent:** _Awaiting assignment_
**Date:** _Pending_

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story creation | @architect (Aria) |

---

**Criado por:** Aria (@architect)
**Data:** 2026-02-20
**Atualizado:** 2026-02-20 (Initial creation)
