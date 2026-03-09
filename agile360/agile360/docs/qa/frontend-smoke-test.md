# Teste de fumaça – Frontend (Landing + Dashboard)

## Pré-requisitos

- Node.js 18+ instalado
- Terminal aberto na **pasta do projeto** (ex.: `C:\agile360`)

## 1. Subir o servidor de desenvolvimento

O servidor do frontend **não** fica na raiz do repositório. É preciso entrar na pasta `frontend` e rodar o script de desenvolvimento:

```powershell
cd frontend
npm install
npm run dev
```

**Se você rodar `npm run dev` na raiz (`C:\agile360`), o comando não será encontrado** e a porta 5173 não ficará em uso — por isso o navegador mostra "Não é possível acessar esse site".

### Saída esperada no terminal

Algo como:

```
  VITE v5.x.x  ready in xxx ms
  ➜  Local:   http://localhost:5173/
  ➜  Network: http://192.168.x.x:5173/
```

Enquanto essa mensagem estiver visível, o servidor está no ar.

## 2. Acessar no navegador

- Abra: **http://localhost:5173/** ou **http://127.0.0.1:5173/**
- A **landing** deve carregar: hero "Visão 360 e Esforço Zero", botões "Começar agora" e "Cadastrar", seção #beneficios.

Se ainda aparecer "Não é possível acessar esse site":

1. Confirme que o terminal mostra "Local: http://localhost:5173/".
2. Tente **127.0.0.1:5173** em vez de localhost.
3. Verifique se outra aplicação não está usando a porta 5173 (em PowerShell: `netstat -ano | findstr :5173`).

## 3. Roteamento e auth (checklist rápido)

| Ação | Resultado esperado |
|------|--------------------|
| Clicar em "Cadastrar" | Vai para `/register` |
| Clicar em "Começar agora" ou "Login" | Vai para `/login` |
| Em `/login`, preencher e-mail/senha inválidos e enviar | Mensagem de erro na tela |
| Em `/login`, preencher credenciais válidas e enviar | Redirecionamento para `/app` (dashboard) |
| No dashboard, clicar em Clientes / Processos / Audiências / Prazos | Página correspondente com título e botão "+ Novo X" |
| Clicar em "Sair" na sidebar | Volta ao estado não logado; ao acessar `/app` redireciona para `/login` |
| Acessar **http://localhost:5173/app** sem estar logado | Redirecionamento para `/login?returnUrl=...` |

## 4. API (opcional)

Para testar login/cadastro de verdade, a API precisa estar rodando (ex.: `dotnet run` no projeto `Agile360.API`). O frontend em dev faz proxy de `/api` para `http://localhost:5000` (ver `frontend/vite.config.ts`).

---

**Resumo:** O erro "Não é possível acessar esse site" na porta 5173 costuma ocorrer quando o servidor **não foi iniciado** — em geral porque `npm run dev` foi executado na **raiz** do repo em vez de dentro da pasta **`frontend`**.
