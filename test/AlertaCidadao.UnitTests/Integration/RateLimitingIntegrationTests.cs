using System.Net;
using AlertaCidadao.API.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaCidadao.UnitTests.Integration;

public sealed class RateLimitingIntegrationTests
{
    [Fact]
    public async Task PoliticaLogin_DeveBloquearAposDuasRequisicoesERetornarRetryAfter()
    {
        await using var app = await CriarAplicacaoAsync(limiteGlobal: 100, limiteLogin: 2);
        var client = app.GetTestClient();

        var primeira = await client.PostAsync("/login", null);
        var segunda = await client.PostAsync("/login", null);
        var terceira = await client.PostAsync("/login", null);

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);
        Assert.True(terceira.Headers.TryGetValues("Retry-After", out var valores));
        Assert.Contains(valores!, valor => int.TryParse(valor, out var segundos) && segundos > 0);
    }

    [Fact]
    public async Task LimiteGlobal_DeveBloquearAposDuasRequisicoes()
    {
        await using var app = await CriarAplicacaoAsync(limiteGlobal: 2, limiteLogin: 100);
        var client = app.GetTestClient();

        var primeira = await client.GetAsync("/recurso");
        var segunda = await client.GetAsync("/recurso");
        var terceira = await client.GetAsync("/recurso");

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);
    }

    private static async Task<WebApplication> CriarAplicacaoAsync(int limiteGlobal, int limiteLogin)
    {
        var configuracao = new Dictionary<string, string?>
        {
            ["RateLimiting:Global:PermitLimit"] = limiteGlobal.ToString(),
            ["RateLimiting:Global:WindowSeconds"] = "60",
            ["RateLimiting:Global:QueueLimit"] = "0",
            ["RateLimiting:Login:PermitLimit"] = limiteLogin.ToString(),
            ["RateLimiting:Login:WindowSeconds"] = "60",
            ["RateLimiting:Login:QueueLimit"] = "0",
            ["RateLimiting:Cadastro:PermitLimit"] = "100",
            ["RateLimiting:Cadastro:WindowSeconds"] = "60",
            ["RateLimiting:Cadastro:QueueLimit"] = "0"
        };

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(configuracao);
        builder.Services.AddApiRateLimiting(builder.Configuration);

        var app = builder.Build();
        app.UseRouting();
        app.UseRateLimiter();
        app.MapPost("/login", () => Results.Ok()).RequireRateLimiting("Login");
        app.MapGet("/recurso", () => Results.Ok());
        await app.StartAsync();
        return app;
    }
}
