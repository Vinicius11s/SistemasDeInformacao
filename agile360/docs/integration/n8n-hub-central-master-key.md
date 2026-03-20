# Hub Central × n8n — Master Key + `X-On-Behalf-Of`

Este fluxo substitui a abordagem de `X-Api-Key` por **Master Service Key** (env var no backend) e **tenant resolution** pelo header `X-On-Behalf-Of`.

## Visão geral
1. A Evolution envia webhook para o n8n.
2. O n8n extrai o número do remetente.
3. O n8n consulta `public.advogado_whatsapp` para resolver o **tenant**:
   - `id_advogado` (UUID) via `whatsapp_numero` (número do remetente).
4. O n8n chama `POST /api/cliente/staging` com:
   - `X-Master-Service-Key: <valor>`
   - `X-On-Behalf-Of: <id_advogado>`
5. O backend injeta `advogado_id` via claims e preenche `AdvogadoId` na staging. O JSON não deve enviar `advogado_id`.

## Pré-requisitos
### Backend (env var)
Configure no backend:
- `MasterServiceKey` (string secreta)

### Tabela de mapeamento
Garanta que existe (schema já criado no projeto):
- `public.advogado_whatsapp`
  - mapeia `id_advogado` (UUID) para o `whatsapp_numero` (texto/dígitos)

E crie/garanta índice para lookup por número:
- índice em `whatsapp_numero`

## Passo a passo — n8n

### 1) Extrair número do remetente (Evolution)
Use algo equivalente:

```js
{{ $json.from.replace('@s.whatsapp.net', '').replace(/\D/g, '') }}
```

Ex.: `5511912345678`

### 2) Consultar tenant no Supabase (advogado_whatsapp)
Nó **Supabase** (ou HTTP para PostgREST):

- Operação: `SELECT`
- Tabela: `public.advogado_whatsapp`
- Filtro: `whatsapp_numero = {{ $json.numeroRemetente }}`
- Campos: `id_advogado`

Salve em variável:
- `advogadoId = {{ $json[0].id_advogado }}`

Se não achar:
- pare o workflow e responda no WhatsApp “Número não cadastrado”.

### 3) Chamar a Staging

Nó **HTTP Request**:
- Método: `POST`
- URL: `https://<SEU_DOMINIO>/api/cliente/staging`
- Headers:
  - `X-Master-Service-Key: {{ $env.MASTER_SERVICE_KEY }}`
  - `X-On-Behalf-Of: {{ $vars.advogadoId }}`
  - `Content-Type: application/json`

Body (exatamente o `CreateStagingClienteRequest`):
- Não envie `advogado_id` no body.
- Envie apenas os campos de cadastro (nome/CPF/CNPJ/endereço/origem/mensagem...).

## Segurança
- A API valida a Master Key antes de aceitar `X-On-Behalf-Of`.
- O `advogado_id` usado na staging vem de `Claims` (derivadas do header), não do JSON.
- n8n não precisa possuir/chamar `X-Api-Key` por advogado.

