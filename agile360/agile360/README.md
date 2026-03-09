# Agile360

**CRM Jurídico – Visão 360 e Esforço Zero**

Sistema para advogados que centraliza clientes, processos, audiências e prazos, com integração WhatsApp, monitor de intimações e guardião de prazos.

[![CI](https://github.com/OWNER/REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/ci.yml)  
*(Substitua OWNER/REPO pelo seu org/repositório no GitHub.)*

---

## Stack tecnológica

| Camada        | Tecnologia        |
|---------------|-------------------|
| Backend API   | .NET 9 (C#)       |
| Banco de dados| Supabase (PostgreSQL) |
| ORM           | Entity Framework Core 9 |
| Documentação API | Swagger/OpenAPI |
| Logging       | Serilog           |
| Padrões       | Clean Architecture, CQRS (MediatR), Repository, DTOs |

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL (local ou Supabase)
- (Opcional) Docker e Docker Compose

---

## Setup local

1. **Clone e entre na pasta do projeto**
   ```bash
   cd agile360
   ```

2. **Configure variáveis de ambiente**
   - Copie `.env.example` para `.env`
   - Preencha `ConnectionStrings__Supabase` com a connection string do seu projeto Supabase (ou PostgreSQL local)

3. **Restore e build**
   ```bash
   dotnet restore Agile360.sln
   dotnet build Agile360.sln -c Release
   ```

4. **Execute a API**
   ```bash
   dotnet run --project src/Agile360.API
   ```
   - API: `http://localhost:5000` (ou porta indicada no console)
   - Swagger: `http://localhost:5000/swagger`
   - Health: `http://localhost:5000/api/health` ou `http://localhost:5000/health`

5. **Frontend (React + Vite)** – rodar **dentro da pasta `frontend`**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   - App: `http://localhost:5173/`  
   - Se aparecer "Não é possível acessar esse site", confira que o comando foi executado em `frontend` (não na raiz do repo). Ver também `docs/qa/frontend-smoke-test.md`.

---

## Estrutura de pastas

```
agile360/
├── frontend/                    # React + Vite (landing, auth, dashboard)
├── src/
│   ├── Agile360.API/           # Controllers, Middleware, Program.cs
│   ├── Agile360.Application/    # Use Cases, DTOs, Validators, MediatR
│   ├── Agile360.Domain/         # Entities, Interfaces (sem deps externas)
│   ├── Agile360.Infrastructure/# EF Core, Repositories, DbContext
│   └── Agile360.Shared/         # Extensions, Helpers, Constants
├── docs/                        # Stories, PRD, Architecture, QA
├── Agile360.sln
├── global.json
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## Docker

```bash
docker compose up --build
```

A API sobe na porta **8080**. Health: `http://localhost:8080/health`.

Configure as variáveis em `.env` ou em `environment` no `docker-compose.yml`.

---

## Cliente TypeScript (OpenAPI)

Para o frontend consumir a API com tipos alinhados ao contrato:

- **Opção 1:** NSwag – gerar cliente a partir do Swagger em build ou script.
- **Opção 2:** OpenAPI Generator – `npx @openapitools/openapi-generator-cli generate -i http://localhost:5000/swagger/v1/swagger.json -g typescript-fetch -o frontend/src/api/generated`

Output sugerido: `frontend/src/api/generated/` (a ser usado na Epic 7). Ver [Gaps e Decisões](docs/architecture/gaps-and-decisions.md).

---

## Padrões de código

- **Clean Architecture:** API → Application → Domain; API → Infrastructure → Application → Domain.
- **DTOs:** Nenhuma entidade do EF Core é exposta na API; usar sempre DTOs da Application.
- **Validação:** FluentValidation em todos os Commands que alteram estado.
- **SRP:** Controllers finos; lógica em Handlers/Services.

---

## CI/CD

- **PRs devem ter CI verde para merge.** O workflow `ci.yml` roda em todo push/PR para `main` e `develop` (build + testes). Ver [Deployment](docs/architecture/deployment.md) para CD (Docker build/push) e variáveis de ambiente.

---

## Documentação

- [Arquitetura do sistema](docs/architecture/system-architecture.md)
- [Revalidação da arquitetura](docs/architecture/architecture-revalidation.md)
- [Gaps e decisões](docs/architecture/gaps-and-decisions.md)
- [Stories](docs/stories/)

---

## Licença

Proprietário – Agile360.
