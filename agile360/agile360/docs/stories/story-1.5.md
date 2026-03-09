# Story 1.5: CI/CD Pipeline – GitHub Actions & Deployment Path

**Epic:** Fundação e Infraestrutura
**Story ID:** 1.5
**Sprint:** 1
**Priority:** 🟠 High
**Points:** 5
**Effort:** 4-6 hours
**Status:** ⚪ Ready
**Type:** 🔧 Infrastructure

---

## 🔀 Cross-Story Decisions

| Decision | Source | Impact on This Story |
|----------|--------|----------------------|
| SaaS na nuvem (Cloudfy/Azure) | Stakeholder | Pipeline de entrega deve estar pronto desde cedo |
| Docker na Story 1.1 | Story 1.1 | CI/CD builda imagem e pode publicar em registry |
| Testes na Story 1.1.1 | Story 1.1.1 | Pipeline deve rodar `dotnet test` e falhar se testes falharem |

---

## 📋 User Story

**Como** desenvolvedor do Agile360,
**Quero** um pipeline de CI/CD com GitHub Actions que builda, testa e prepara o deploy da API,
**Para** não depender de deploy manual via Docker e ter um caminho claro para subir o SaaS na nuvem (Cloudfy, Azure ou outro).

---

## 🎯 Objective

Configurar o fluxo de entrega contínua com GitHub Actions: build da solução .NET 9, execução de testes (unit + integration), lint/format check (opcional), build da imagem Docker e push para um container registry. Deixar preparado (com jobs ou workflows separados) o caminho para deploy em ambiente de staging/produção (Azure Container Apps, App Service, Cloudfy ou equivalente), sem implementar o deploy automático para prod nesta story.

---

## ✅ Tasks

### Phase 1: CI – Build & Test (~2h)

- [ ] **1.1** Criar workflow `.github/workflows/ci.yml`:
  - Trigger: push em `main`, `develop`; pull_request para `main`, `develop`
  - Job: `build-and-test`
  - Runner: `ubuntu-latest` (ou windows-latest se preferir; ubuntu é comum para .NET)
- [ ] **1.2** Steps do job:
  - Checkout
  - Setup .NET 9 SDK (action: actions/setup-dotnet; dotnet-version: 9.0.x)
  - Restore: `dotnet restore`
  - Build: `dotnet build --no-restore -c Release`
  - Test: `dotnet test --no-build -c Release --verbosity normal`
  - (Opcional) Coverlet: publicar resultado de cobertura como artifact ou em dashboard
- [ ] **1.3** Garantir que o job falha se algum teste falhar (comportamento padrão)
- [ ] **1.4** Cache de dependências NuGet:
  - Usar `actions/cache` com chave baseada em `**/packages.lock.json` ou `**/*.csproj`
  - Restore mais rápido em PRs
- [ ] **1.5** (Opcional) Lint: `dotnet format --verify-no-changes` ou analisadores; falhar se código fora do padrão

### Phase 2: CD – Docker Build & Push (~2h)

- [ ] **2.1** Criar workflow `.github/workflows/cd.yml` (ou job no mesmo arquivo com `if`):
  - Trigger: push em `main` com tag ou path; ou push em tag `v*`
  - Evitar rodar em todo push; ex.: apenas quando `src/Agile360.API/**` ou `Dockerfile` mudam
- [ ] **2.2** Job `docker-build`:
  - Checkout
  - Login em Container Registry:
    - GitHub Container Registry (ghcr.io): usar `GITHUB_TOKEN` ou PAT
    - Ou Azure ACR / Docker Hub: secrets `REGISTRY_USERNAME`, `REGISTRY_PASSWORD`
  - Build da imagem: `docker build -t <registry>/agile360-api:latest .` (ou tag com SHA/tag do git)
  - Push: `docker push <registry>/agile360-api:latest`
- [ ] **2.3** Usar build cache (Docker Buildx ou cache-from) para acelerar builds
- [ ] **2.4** Documentar no README: quais secrets configurar no GitHub para CD (REGISTRY_*, etc.)

### Phase 3: Deployment Path (Documentação + Stub) (~1h)

- [ ] **3.1** Documentar em `docs/architecture/deployment.md` (ou seção no system-architecture):
  - Opção Azure: Container Apps ou App Service com container; variáveis de ambiente
  - Opção Cloudfy: passos sugeridos (link ou resumo)
  - Opção genérica: “qualquer host que rode Docker” com env vars
- [ ] **3.2** Criar job ou workflow “deploy-staging” (stub ou manual):
  - Ex.: `workflow_dispatch` para deploy manual para staging
  - Ou deploy automático para staging quando push em `develop`
  - Não fazer deploy automático para produção nesta story
- [ ] **3.3** Listar variáveis de ambiente necessárias em produção (ConnectionStrings, JwtSettings, Webhooks:N8nSecret, etc.) em `docs/architecture/deployment.md`

### Phase 4: Badge e Conventions (~0.5h)

- [ ] **4.1** Adicionar badge no README: “CI” (build + test) apontando para o workflow
- [ ] **4.2** Documentar no README ou CONTRIBUTING: “PRs devem ter CI verde para merge”
- [ ] **4.3** (Opcional) Branch protection em `main`: exigir status de CI para merge

---

## 🎯 Acceptance Criteria

```gherkin
GIVEN um push ou PR para main/develop
WHEN o workflow CI é disparado
THEN a solução é restaurada, buildada em Release e os testes são executados
AND o job falha se o build ou qualquer teste falhar

GIVEN o CD está configurado (ex.: push em main)
WHEN o workflow CD é disparado
THEN a imagem Docker é buildada
AND a imagem é enviada para o registry configurado
AND não há credenciais hardcoded (apenas secrets)

GIVEN a documentação de deployment existe
WHEN um desenvolvedor lê docs/architecture/deployment.md
THEN encontra o caminho para deploy em Azure/Cloudfy e lista de env vars
AND há um stub ou instrução para deploy em staging
```

---

## 🤖 CodeRabbit Integration

### Story Type Analysis

| Attribute | Value | Rationale |
|-----------|-------|-----------|
| Type | Infrastructure | Pipeline de entrega |
| Complexity | Low–Medium | Workflows padrão; sem deploy prod complexo |
| Test Requirements | N/A (pipeline é o “test” do processo) | CI roda testes existentes |
| Review Focus | Security | Secrets; não expor tokens em logs |

### Focus Areas

- [ ] Nenhum secret em plain text no workflow; usar GitHub Secrets
- [ ] Dockerfile multi-stage já existe (Story 1.1); apenas referenciar no CD
- [ ] Cache de NuGet e Docker para tempo de build menor

---

## 🔗 Dependencies

**Blocked by:**
- Story 1.1: Project Scaffolding (Dockerfile, estrutura da solução)
- Story 1.1.1: Test Foundation (dotnet test com projetos de teste)

**Blocks:**
- Deploy contínuo para staging/prod (esta story prepara o caminho; deploy real pode ser story futura)

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Testes de integração exigem DB no CI | Medium | Testcontainers ou banco de serviço no GitHub; ou marcar testes pesados como “[Trait]” e rodar só em CD |
| Registry pago ou limite | Low | GHCR é gratuito para repos públicos; documentar limites |
| Secret vazado em log | High | Não fazer echo de secrets; usar masked vars |

---

## 📋 Definition of Done

- [ ] Workflow CI (build + test) existe e passa em main/develop
- [ ] Workflow CD (Docker build + push) existe e usa secrets para registry
- [ ] Documentação de deployment com env vars e opções (Azure/Cloudfy)
- [ ] Badge de CI no README
- [ ] All acceptance criteria verified

---

## 📝 Dev Notes

### Key Files

```
.github/
├── workflows/
│   ├── ci.yml
│   └── cd.yml
docs/
└── architecture/
    └── deployment.md
```

### Secrets sugeridos (GitHub)

- `REGISTRY_USERNAME` / `REGISTRY_PASSWORD` (ou `GITHUB_TOKEN` para GHCR)
- Para deploy futuro: `AZURE_CREDENTIALS`, `AZURE_WEBAPP_NAME`, etc.

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 1.0.0 | Initial story – CI/CD Pipeline | @architect (Aria) |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20
