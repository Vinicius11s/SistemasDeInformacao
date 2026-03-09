# Agile360 – Convenções de Testes

**Story:** 1.1.1 (Test Foundation)

---

## Nomenclatura

- **Padrão:** `MethodName_Scenario_ExpectedBehavior`
- Exemplo: `GetById_ClienteOfOtherTenant_ReturnsNull`

---

## Unit Tests

- **Sem I/O:** Não acessar disco, rede ou banco.
- **Mocks:** Usar NSubstitute para todas as dependências externas (IRepository, IUnitOfWork, ITenantProvider).
- **Builders:** Usar AdvogadoBuilder, ClienteBuilder, ProcessoBuilder para entidades de teste.
- **TestBase:** Herdar ou usar helpers de TestBase para mocks comuns.

---

## Integration Tests

- **DB:** Podem usar banco de teste (SQLite in-memory nos testes de isolamento; config no WebApplicationFactory).
- **Estado:** Isolar estado por teste; usar seed + cleanup ou DB novo por teste para evitar flaky.
- **Multi-Tenancy:** Testes de isolamento (TenantIsolationTests) validam que Advogado A nunca vê dados do Advogado B.

---

## Cenários de Data Leak (regressão)

- Query Filter: listar entidades retorna apenas as do tenant corrente.
- GetById: buscar por ID de entidade de outro tenant retorna null (API deve retornar 404).
- Insert: novo registro recebe AdvogadoId do tenant corrente (TenantSaveChangesInterceptor).

---

## Execução

```bash
dotnet test Agile360.sln -c Release
```

Testes devem passar para o build ser considerado OK (CI – Story 1.5).
