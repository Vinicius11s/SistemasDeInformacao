# Agile360 – Deployment & Variáveis de Ambiente

**Versão:** 1.0.0 (Story 1.5)  
**Criado por:** Aria (@architect)  
**Data:** 2026-02-20

---

## 📋 Objetivo

Este documento descreve o **caminho para deploy** do Agile360 na nuvem (Azure, Cloudfy ou host genérico) e lista as **variáveis de ambiente** necessárias em produção.

---

## 🔄 CI/CD (GitHub Actions)

- **CI** (`.github/workflows/ci.yml`): dispara em push/PR para `main` e `develop`. Restaura dependências, builda em Release e executa `dotnet test`. Cache de NuGet para acelerar. O job falha se build ou testes falharem.
- **CD** (`.github/workflows/cd.yml`): dispara em push para `main` (ou manualmente) quando alteram `src/Agile360.API/**`, `Dockerfile` ou o próprio workflow. Faz build da imagem Docker e push para o **GitHub Container Registry** (ghcr.io). Usa `GITHUB_TOKEN` (não é necessário configurar secrets para GHCR). Para outro registry (Azure ACR, Docker Hub), configurar secrets `REGISTRY_USERNAME` e `REGISTRY_PASSWORD` e ajustar o workflow.
- **Deploy para staging:** Pode ser adicionado um job ou workflow separado (ex.: `workflow_dispatch` ou trigger em push para `develop`) que faz deploy da imagem para um ambiente de staging. Não implementado nesta story.
- **Deploy para produção:** Não há deploy automático para produção; seguir uma das opções abaixo de forma manual ou em story futura.

---

## 🌐 Opções de Deploy

| Plataforma | Uso sugerido | Observação |
|------------|--------------|------------|
| **Azure Container Apps** | Produção / Staging | Rodar imagem Docker da API; scaling automático |
| **Azure App Service** | Produção | Container ou deploy direto .NET; fácil integração com ACR |
| **Cloudfy** | Produção | Seguir documentação Cloudfy para container/API; configurar env vars abaixo |
| **Qualquer host Docker** | Staging / on-premise | `docker run` com env vars; garantir HTTPS e secrets |

---

## 🔐 Variáveis de Ambiente (Produção)

As variáveis abaixo devem ser configuradas no ambiente de execução (nunca em código).

### Obrigatórias

| Variável | Descrição | Exemplo (não usar em prod) |
|----------|-----------|----------------------------|
| `ConnectionStrings__Supabase` | Connection string PostgreSQL (Supabase) | `Host=xxx.supabase.co;Port=5432;Database=postgres;...` |
| `JwtSettings__Secret` | JWT secret do Supabase Auth | (valor do dashboard Supabase) |
| `JwtSettings__Issuer` | Issuer do token | `https://<project>.supabase.co/auth/v1` |
| `JwtSettings__Audience` | Audience | `authenticated` |
| `Webhooks__N8nSecret` | Secret para validar webhooks do n8n | (string forte; HMAC) |
| `CORS__AllowedOrigins` | Origens permitidas (frontend) | `https://app.agile360.com` |

### Opcionais / Integrações

| Variável | Descrição |
|----------|-----------|
| `N8n__BaseUrl` | URL base do n8n (chamadas outbound da API) |
| `N8n__ApiKey` | API Key para autenticar chamadas da API ao n8n |
| `Serilog__MinimumLevel` | Nível de log (Information / Warning / Debug) |
| `ASPNETCORE_ENVIRONMENT` | Production / Staging / Development |
| `ASPNETCORE_URLS` | URLs de escuta (ex.: `http://+:8080`) |

---

## 📜 Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-20 | 0.1.0 | Stub criado | @architect (Aria) |
| 2026-02-21 | 1.0.0 | Story 1.5: CI/CD workflows, variáveis de ambiente, opções de deploy | @dev |

---

**Criado por:** Aria (@architect)  
**Data:** 2026-02-20
