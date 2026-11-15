# Testes automatizados

## Escopo

A suíte cobre os recursos de segurança e resiliência adicionados à API:

| Recurso | Cenários automatizados |
|---|---|
| JWT | Emissão e claims, autenticação sem token, token adulterado, autorização de cidadão e gestor |
| Idempotência | Repetição com mesma chave e payload, conflito com payload diferente, ausência e chave inválida |
| Rate Limiting | Limite global, política de Login, HTTP 429 e `Retry-After` |
| Health Checks | Liveness, Readiness saudável, Readiness indisponível e endpoint agregado |

## Execução

Na raiz do repositório:

```powershell
dotnet restore .\AlertaCidadao.sln
dotnet test .\AlertaCidadao.sln --configuration Release --logger "console;verbosity=normal"
```

## Limites dos testes de idempotência

Os testes de replay e conflito usam EF Core InMemory para isolar o filtro e validar o comportamento sequencial. Esse provedor não reproduz bloqueios de índice único, transações Oracle ou concorrência real entre instâncias. A atomicidade e a disputa simultânea pela mesma chave devem ser exercitadas em um ambiente de integração conectado ao Oracle.
