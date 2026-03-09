# QA – Clean Data: Testes de Sanitização de Documentos

## Objetivo
Garantir que CPF, CNPJ, RG, Telefone e WhatsApp sejam sempre persistidos
como sequências de dígitos puros, independentemente do formato enviado.

---

## 1. Testes de Stress via API (Postman / curl)

Para cada caso abaixo, envie um `POST /api/clientes` e inspecione a linha
no Supabase (`SELECT cpf, cnpj, rg FROM clientes ORDER BY created_at DESC LIMIT 1`).

| # | Input enviado          | Esperado no banco | Deve salvar? |
|---|------------------------|-------------------|--------------|
| 1 | `"123.456.789-09"`     | `12345678909`     | ✅ sim        |
| 2 | `"123456789-09"`       | `12345678909`     | ✅ sim        |
| 3 | `"12345678909"`        | `12345678909`     | ✅ sim        |
| 4 | `"123 456 789 09"`     | `12345678909`     | ✅ sim        |
| 5 | `"12.345.678/0001-95"` | `12345678000195`  | ✅ sim        |
| 6 | `"12345678000195"`     | `12345678000195`  | ✅ sim        |
| 7 | `"CPF: 123.456.789-09"`| `12345678909`     | ✅ (letras ignoradas) |
| 8 | `" "`  (só espaço)     | `NULL`            | ✅ nulo       |
| 9 | `"000.000.000-00"`     | rejeitado pelo validador (CPF inválido) | ✅ |
|10 | Duplicata CPF (mesmo advogadoId) | `409 Conflict` esperado | ✅ |
|11 | Duplicata CNPJ com máscara vs sem | deve detectar como duplicata | ✅ |

---

## 2. Validação Direta no Supabase

Após cada inserção, execute no SQL Editor:

```sql
-- Checar se há qualquer não-dígito em documentos
SELECT id, cpf, cnpj, rg, telefone
FROM clientes
WHERE
    cpf   ~ '[^\d]' OR
    cnpj  ~ '[^\d]' OR
    rg    ~ '[^\d]' OR
    telefone ~ '[^\d]';
-- Resultado esperado: 0 linhas
```

---

## 3. Teste de Deduplicação

```sql
-- Caso de teste: inserir 2x o mesmo CPF com formatos diferentes
-- POST 1: { "cpf": "123.456.789-09", ... }
-- POST 2: { "cpf": "12345678909",    ... }
-- A API deve retornar 409 na segunda requisição.
-- Verificar que há apenas 1 linha com esse CPF no banco:
SELECT COUNT(*) FROM clientes WHERE cpf = '12345678909';
-- Esperado: 1
```

---

## 4. Teste de Exibição no Frontend

1. Abrir `/app/clientes`
2. Os documentos na coluna CPF/CNPJ devem aparecer formatados:
   - `12345678909`     → `123.456.789-09`
   - `12345678000195`  → `12.345.678/0001-95`
3. Inspecionar DevTools → Network: verificar que a requisição de criação
   envia o valor **sem máscara** (ex.: `"cpf":"12345678909"`)

---

## 5. Teste de Silent Fail (letras acidentais)

Cenário: operador cola `"CPF: 123.456.789-09"` no campo.
- O input mask remove qualquer não-dígito em tempo real
- O campo exibe `123.456.789-09` ao terminar de digitar
- O payload enviado é `"12345678909"`

---

## Critérios de Aceitação

- [ ] Nenhuma coluna de documento contém caractere não-dígito no banco
- [ ] Deduplicação funciona independentemente do formato recebido
- [ ] Frontend exibe documentos com máscara; payload enviado é sempre raw
- [ ] Validator rejeita CPF/CNPJ com dígitos inválidos (algoritmo)
- [ ] Validator aceita tanto `123.456.789-09` quanto `12345678909`
