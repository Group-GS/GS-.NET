# Testes automatizados — AlertaCidadao

A suíte automatizada fica em `test/AlertaCidadao.UnitTests` e utiliza xUnit.

## Cobertura

- **JWT:** geração do token, claims, expiração, autenticação Bearer, rejeição de token ausente/adulterado e autorização por perfil.
- **Idempotência:** replay da resposta original para a mesma chave e payload, conflito para payload diferente, chave ausente e chave inválida.
- **Rate Limiting:** política específica de Login, limite global, HTTP 429 e cabeçalho `Retry-After`.
- **Health Checks:** Liveness, Readiness, endpoint agregado, Oracle acessível e Oracle indisponível.
- **Demais regras:** serviços de usuário, hash PBKDF2 e validação de DTOs.

Os testes de integração HTTP usam `Microsoft.AspNetCore.TestHost`. Os testes do filtro de idempotência usam o provedor EF Core InMemory para validar replay e conflito sequencial; a garantia de atomicidade e concorrência real depende do índice único e das transações Oracle e deve ser verificada também em ambiente Oracle de integração.

## Executar no PowerShell

Na raiz da solução:

```powershell
dotnet restore .\AlertaCidadao.sln
dotnet test .\AlertaCidadao.sln --configuration Release --logger "console;verbosity=normal"
```

Para executar somente os testes adicionados:

```powershell
dotnet test .\test\AlertaCidadao.UnitTests\AlertaCidadao.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~JwtAuthenticationIntegrationTests|FullyQualifiedName~IdempotencyEndpointFilterTests|FullyQualifiedName~RateLimitingIntegrationTests|FullyQualifiedName~HealthCheckEndpointTests"
```

Os testes não exigem uma conexão Oracle ativa. O teste de Health Check simula a ausência do contexto para validar a resposta de indisponibilidade e usa EF Core InMemory para validar o caminho saudável.
