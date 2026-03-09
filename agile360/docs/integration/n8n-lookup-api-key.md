# n8n – Lookup automático de API Key pelo número do WhatsApp

## Visão geral

O admin cadastra a chave uma única vez no banco. O n8n busca automaticamente
antes de cada requisição à API. O advogado nunca vê ou digita a chave.

```
WhatsApp → n8n
  1. Extrai número do remetente: {{ $json.from }}  (ex: "5511912345678")
  2. Nó "Supabase" → SELECT n8n_api_key FROM advogado WHERE telefone = '5511912345678'
  3. Armazena em variável: apiKey = {{ $json[0].n8n_api_key }}
  4. Nó "HTTP Request" → Header: X-Api-Key = {{ $vars.apiKey }}
     PUT /api/clientes/{id}  com o JSON de alteração
```

---

## Passo 1 — Admin gera a chave (uma vez por advogado)

```http
POST /api/api-keys
Authorization: Bearer <jwt_do_admin_ou_do_proprio_advogado>
Content-Type: application/json

{ "name": "n8n WhatsApp" }
```

Resposta: copiar o campo `rawKey` (ex: `a360_x7kqz0abc123...`)

---

## Passo 2 — Admin grava no banco

```sql
UPDATE public.advogado
SET n8n_api_key = 'a360_x7kqz0abc123...'
WHERE email = 'advogado@escritorio.com.br';
```

---

## Passo 3 — Configurar n8n

### Nó 1: Extrair número do WhatsApp
```
Tipo: Set
Nome: Número do remetente
Valor: {{ $json.from.replace('@s.whatsapp.net', '').replace(/\D/g, '') }}
```

### Nó 2: Buscar chave no Supabase
```
Tipo: Supabase (ou HTTP Request para a API REST do Supabase)
Operação: SELECT
Tabela: advogado
Filtro: telefone = {{ $json.numeroRemetente }}
Campos: n8n_api_key
```

### Nó 3: Guardar chave em variável de execução
```
Tipo: Set
Nome: apiKey
Valor: {{ $json[0].n8n_api_key }}
```

### Nó 4: Chamar a API do Agile360
```
Tipo: HTTP Request
Método: PUT
URL: https://api.agile360.com.br/api/clientes/{{ $json.clienteId }}
Headers:
  X-Api-Key: {{ $('Buscar chave').item.json.n8n_api_key }}
  Content-Type: application/json
Body: { "status": "Ativo", ... }
```

---

## Tratamento de erro: advogado não encontrado

Se o SELECT retornar vazio, o número não está cadastrado.
Adicionar um nó condicional:

```
IF {{ $json[0]?.n8n_api_key }} existe
  → continuar fluxo
ELSE
  → responder no WhatsApp: "Número não cadastrado. Entre em contato com o suporte."
  → STOP (não chama a API)
```

---

## Segurança desta abordagem

| Aspecto | Avaliação |
|---|---|
| Advogado vê a chave? | Não — apenas o admin acessa o banco |
| Chave exposta no WhatsApp? | Impossível — o n8n busca internamente |
| Pode ser adivinhada? | Não — 62^32 combinações (impossível por força bruta) |
| n8n já tem acesso ao banco? | Sim (service_role) — superfície de ataque não aumenta |
| O que acontece se o n8n for comprometido? | Revogar a chave: `DELETE /api/api-keys/{id}` — efeito imediato |
