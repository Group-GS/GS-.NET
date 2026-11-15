using System.Net;
using System.Net.Http.Json;
using AlertaCidadao.API.Filters;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaCidadao.UnitTests.Idempotency;

public sealed class IdempotencyEndpointFilterTests
{
    [Fact]
    public async Task MesmaChaveEMesmoPayload_DeveReproduzirRespostaOriginalSemExecutarNovamente()
    {
        var execucoes = 0;
        await using var app = await CriarAplicacaoAsync(() => Interlocked.Increment(ref execucoes));
        var client = app.GetTestClient();

        var primeira = await EnviarAsync(client, "chave-repetida", "Maria");
        var segunda = await EnviarAsync(client, "chave-repetida", "Maria");

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Equal(await primeira.Content.ReadAsStringAsync(), await segunda.Content.ReadAsStringAsync());
        Assert.Equal(1, execucoes);
    }

    [Fact]
    public async Task MesmaChaveComPayloadDiferente_DeveRetornar409SemReexecutar()
    {
        var execucoes = 0;
        await using var app = await CriarAplicacaoAsync(() => Interlocked.Increment(ref execucoes));
        var client = app.GetTestClient();

        var primeira = await EnviarAsync(client, "chave-conflito", "Maria");
        var segunda = await EnviarAsync(client, "chave-conflito", "Joana");

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
        Assert.Equal(1, execucoes);
    }

    [Fact]
    public async Task ChaveAusente_DeveManterComportamentoSemIdempotencia()
    {
        var execucoes = 0;
        await using var app = await CriarAplicacaoAsync(() => Interlocked.Increment(ref execucoes));
        var client = app.GetTestClient();

        var primeira = await EnviarAsync(client, null, "Maria");
        var segunda = await EnviarAsync(client, null, "Maria");

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Equal(2, execucoes);
    }

    [Fact]
    public async Task ChaveAcimaDoLimite_DeveRetornar400SemExecutarOperacao()
    {
        var execucoes = 0;
        await using var app = await CriarAplicacaoAsync(
            () => Interlocked.Increment(ref execucoes));

        var client = app.GetTestClient();

        // O limite permitido é de 100 caracteres.
        var chaveInvalida = new string('a', 101);

        var response = await EnviarAsync(
            client,
            chaveInvalida,
            "Maria");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, execucoes);
    }

    private static async Task<HttpResponseMessage> EnviarAsync(
        HttpClient client,
        string? chave,
        string nome)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/idempotente")
        {
            Content = JsonContent.Create(new CriarUsuarioRequest
            {
                Nome = nome,
                Email = $"{nome.ToLowerInvariant()}@example.com",
                Senha = "SenhaSegura123!",
                Perfil = "CIDADAO"
            })
        };

        if (chave is not null)
            request.Headers.TryAddWithoutValidation(IdempotencyEndpointFilter.HeaderName, chave);

        return await client.SendAsync(request);
    }

    private static async Task<WebApplication> CriarAplicacaoAsync(Func<int> incrementar)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var databaseName = $"idempotencia-{Guid.NewGuid():N}";
        builder.Services.AddDbContext<AlertaCidadaoDbContext>(options =>
            options.UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

        var app = builder.Build();
        app.MapPost("/idempotente", (CriarUsuarioRequest request) =>
        {
            var numero = incrementar();
            return Results.Ok(new { numero, request.Nome });
        }).AddEndpointFilter<IdempotencyEndpointFilter>();

        await app.StartAsync();
        return app;
    }
}
