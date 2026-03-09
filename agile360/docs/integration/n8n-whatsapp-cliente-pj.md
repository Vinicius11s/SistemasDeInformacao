# Integração n8n / WhatsApp — Cadastro de Cliente PJ

## Fluxo de perguntas por TipoPessoa

O bot detecta o tipo do cliente na primeira pergunta e ramifica o diálogo.

### Ramo Pessoa Física (`tipo_pessoa = PessoaFisica`)

| Passo | Pergunta do bot                         | Coluna destino     |
|-------|-----------------------------------------|--------------------|
| 1     | "Qual o nome completo do cliente?"      | `nome`             |
| 2     | "Qual o CPF do cliente?"                | `cpf`              |
| 3     | "Qual a **data de nascimento**?"        | `data_referencia`  |
| 4     | "Qual a **profissão** do cliente?"      | `area_atuacao`     |
| 5     | "Qual o telefone/WhatsApp?"             | `telefone` / `whatsapp_numero` |

### Ramo Pessoa Jurídica (`tipo_pessoa = PessoaJuridica`)

| Passo | Pergunta do bot                                    | Coluna destino     |
|-------|----------------------------------------------------|--------------------|
| 1     | "Qual a razão social da empresa?"                  | `razao_social`     |
| 2     | "Qual o CNPJ?"                                     | `cnpj`             |
| 3     | "Qual a **data de abertura/fundação** da empresa?" | `data_referencia`  |
| 4     | "Qual o **ramo de atividade** da empresa?"         | `area_atuacao`     |
| 5     | "Qual o telefone/WhatsApp da empresa?"             | `telefone` / `whatsapp_numero` |

## Regras de sanitização no payload

O nó HTTP do n8n deve enviar os dados já limpos:
- `data_referencia` → formato ISO: `YYYY-MM-DD`
- `cpf` / `cnpj`   → somente dígitos (sem máscara)
- `telefone`        → somente dígitos

## Exemplo de payload para PJ

```json
{
  "tipoPessoa": "PessoaJuridica",
  "razaoSocial": "Empresa XYZ Ltda",
  "cnpj": "12345678000195",
  "dataReferencia": "2010-07-01",
  "areaAtuacao": "Comércio varejista de vestuário",
  "telefone": "1133334444",
  "whatsAppNumero": "11912345678",
  "origem": "WhatsApp"
}
```

## Exemplo de payload para PF

```json
{
  "tipoPessoa": "PessoaFisica",
  "nome": "João da Silva",
  "cpf": "12345678909",
  "dataReferencia": "1985-03-22",
  "areaAtuacao": "Engenheiro Civil",
  "telefone": "11912345678",
  "whatsAppNumero": "11912345678",
  "origem": "WhatsApp"
}
```
