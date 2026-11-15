# Testes automatizados — AlertaCidadao

Esta pasta fica fora de `src` e concentra os testes automatizados da solução.

## Estrutura

```text
test/
└── AlertaCidadao.UnitTests/
    ├── Fakes/
    │   └── FakeUsuarioRepository.cs
    ├── Security/
    │   └── Pbkdf2PasswordHasherTests.cs
    ├── Services/
    │   └── UsuarioServiceTests.cs
    ├── Validation/
    │   └── CriarUsuarioRequestValidationTests.cs
    ├── Usings.cs
    └── AlertaCidadao.UnitTests.csproj
```

## Executar

Na raiz da solução:

```powershell
dotnet restore .\AlertaCidadao.sln
dotnet test .\AlertaCidadao.sln --configuration Release --logger "console;verbosity=normal"
```

Os testes usam repositório falso em memória; não precisam de conexão com Oracle.

## Escopo atual e limitações

Os testes cobrem comportamentos que existem nesta versão do código: geração de hash PBKDF2, regras do cadastro e atualização de usuários, validação de DTOs e ausência de campos de senha/hash no DTO público.

**Esta cópia do projeto ainda não implementa autenticação JWT.** Não há endpoint de login, emissão/validação de token, configuração de autenticação Bearer ou políticas de autorização no `Program.cs`. Por isso, não foram criados testes que aleguem validar JWT. Quando essa implementação for adicionada, devem ser incluídos testes de autenticação e autorização, além de testes de integração HTTP.

A execução dos testes deve ser confirmada no ambiente do desenvolvedor, com .NET SDK 9 instalado e acesso ao NuGet. O ZIP entregue contém o projeto e os casos de teste, mas não representa um resultado de execução local.
