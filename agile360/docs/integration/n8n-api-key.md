# Agile360 × n8n — Autenticação com API Key

## Por que API Key e não JWT?

| | JWT (15 min) | API Key |
|---|---|---|
| Validade | 15 minutos | Configurável / sem expiração |
| Renovação | Necessária a cada 15 min | Nunca (até você revogar) |
| Revogável | Não sem derrubar todos | Sim — instantâneo |
| Segurança | Alta | Alta (hash SHA-256 no banco) |
| Indicado para | Usuário humano no browser | Automações M2M (n8n, bots) |

---

## Passo 1 — Gerar a chave (uma vez, no Postman ou frontend)

Faça login normalmente para obter um JWT de acesso e depois crie a chave:

```http
POST /api/api-keys
Authorization: Bearer <seu_jwt_aqui>
Content-Type: application/json

{
  "name": "n8n Produção",
  "expiresAt": null
}
```

**Resposta (guarde o `rawKey` — ele não aparece novamente):**

```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-...",
    "name": "n8n Produção",
    "keyPrefix": "a360_x7kqz0",
    "rawKey": "a360_x7kqz0abc123def456ghi789jkl012mno345",
    "createdAt": "2026-02-21T10:00:00Z",
    "expiresAt": null
  }
}
```

---

## Passo 2 — Configurar a credencial no n8n

1. No n8n, vá em **Settings → Credentials → New Credential**
2. Escolha o tipo **"Header Auth"**
3. Configure:
   - **Name**: `Agile360 API Key`
   - **Header Name**: `X-Api-Key`
   - **Header Value**: `a360_x7kqz0abc123def456ghi789jkl012mno345`
4. Salve

---

## Passo 3 — Usar em qualquer nó HTTP do n8n

Em cada nó **"HTTP Request"** que chama a API do Agile360:

- **Authentication**: `Header Auth`
- **Credential**: `Agile360 API Key` (criada no passo 2)

O n8n vai adicionar automaticamente `X-Api-Key: a360_...` em todas as requisições.

**Exemplo de nó para criar um cliente:**

```
Method:  POST
URL:     https://sua-api.agile360.com.br/api/clientes
Auth:    Header Auth → Agile360 API Key
Body:
{
  "tipoPessoa": "PessoaJuridica",
  "razaoSocial": "{{ $json.empresa }}",
  "cnpj": "{{ $json.cnpj }}",
  "areaAtuacao": "{{ $json.ramo }}",
  "whatsAppNumero": "{{ $json.telefone }}"
}
```

---

## Listar e revogar chaves

```http
# Listar chaves ativas
GET /api/api-keys
Authorization: Bearer <jwt>

# Revogar uma chave (efeito imediato)
DELETE /api/api-keys/{id}
Authorization: Bearer <jwt>
```

---

## Segurança — o que acontece sob o capô

```
n8n envia: X-Api-Key: a360_x7kqz0...
                ↓
ApiKeyAuthenticationHandler
  SHA-256(rawKey) → hash
  SELECT * FROM api_keys WHERE key_hash = hash AND revoked_at IS NULL
                ↓
  ClaimsPrincipal com AdvogadoId do advogado dono da chave
                ↓
  TenantMiddleware lê AdvogadoId do claim "sub"
                ↓
  HasQueryFilter: todos os dados filtrados por AdvogadoId
                ↓
  FluentValidation + DocumentSanitizer
                ↓
  Repository → Supabase PostgreSQL
```

A chave nunca é armazenada em texto claro — apenas o hash SHA-256. Mesmo que alguém acesse o banco, não consegue recuperar a chave original.
