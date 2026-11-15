# Health Checks — AlertaCidadao API

A API disponibiliza endpoints de Health Checks para acompanhar a disponibilidade do processo e a conectividade com o banco Oracle.

## Endpoints

| Endpoint | Verificação | Quando usar |
|---|---|---|
| `GET /health/live` | Liveness da aplicação (`self`) | Confirma que o processo da API está respondendo. Não depende do Oracle. |
| `GET /health/ready` | Readiness do Oracle (`oracle`) | Confirma que a API consegue estabelecer conexão com o banco Oracle. |
| `GET /health` | Resultado agregado | Retorna todos os Health Checks registrados. |

Os três endpoints são públicos para permitir seu uso por mecanismos de monitoramento e orquestração. Eles também são excluídos do Rate Limiting para que uma sondagem de saúde não seja bloqueada pelas políticas de tráfego da API.

## Códigos HTTP

- `200 OK`: todas as verificações selecionadas estão saudáveis.
- `503 Service Unavailable`: pelo menos uma verificação selecionada está indisponível.

## Exemplo de resposta saudável

```json
{
  "status": "Healthy",
  "durationMs": 24.5,
  "checks": [
    {
      "name": "oracle",
      "status": "Healthy",
      "description": "Conexão com o Oracle disponível.",
      "durationMs": 20.1,
      "tags": ["ready", "database"]
    }
  ]
}
```

Os tempos são ilustrativos. A resposta real apresenta a duração medida durante cada execução. Em caso de falha do Oracle, a API retorna `503` e uma descrição genérica; detalhes técnicos da exceção são mantidos apenas nos logs da aplicação, evitando expor informações de conexão ou credenciais na resposta HTTP.

## Como testar no PowerShell

Com a API em execução, abra um terminal PowerShell e execute:

```powershell
$baseUrl = "http://localhost:5075"

Write-Host "`n--- Liveness ---" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$baseUrl/health/live" -Method Get | ConvertTo-Json -Depth 6

Write-Host "`n--- Readiness do Oracle ---" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$baseUrl/health/ready" -Method Get | ConvertTo-Json -Depth 6

Write-Host "`n--- Health Check agregado ---" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$baseUrl/health" -Method Get | ConvertTo-Json -Depth 6
```

Com o Oracle acessível, os três endpoints devem retornar `200 OK`. Para validar o tratamento de indisponibilidade, em um ambiente local de teste, interrompa temporariamente o acesso ao Oracle e consulte `/health/ready`: o resultado esperado é `503 Service Unavailable`, com `status` igual a `Unhealthy`. O endpoint `/health/live` deve continuar retornando `200 OK`, pois verifica somente se a API está respondendo. Depois do teste, restabeleça a conexão com o Oracle.

Também é possível visualizar os códigos HTTP com:

```powershell
curl.exe -i http://localhost:5075/health/live
curl.exe -i http://localhost:5075/health/ready
curl.exe -i http://localhost:5075/health
```

## Configuração

A verificação Oracle utiliza a mesma instância de `AlertaCidadaoDbContext` e a mesma connection string `ConnectionStrings:Oracle` já configuradas para a aplicação. Não é necessário criar uma segunda conexão nem adicionar credenciais ao `appsettings.json`.

A verificação utiliza `Database.CanConnectAsync`, portanto valida a possibilidade de estabelecer conexão com o banco. Ela não executa migrations, não altera dados e não substitui testes funcionais das tabelas e operações de negócio.
