using System.Net;
using AlertaCidadao.API.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlertaCidadao.UnitTests.HealthChecks;

public sealed class HealthCheckEndpointTests
{
    [Fact]
    public async Task Liveness_DeveRetornar200MesmoQuandoOracleIndisponivel()
    {
        await using var app = await CriarAplicacaoAsync();

        var response = await app.GetTestClient().GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", body);
        Assert.Contains("self", body);
        Assert.DoesNotContain("oracle", body);
    }

    [Fact]
    public async Task Readiness_DeveRetornar503QuandoOracleNaoPuderSerResolvido()
    {
        await using var app = await CriarAplicacaoAsync();

        var response = await app.GetTestClient().GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("Unhealthy", body);
        Assert.Contains("oracle", body);
    }

    [Fact]
    public async Task HealthGeral_DeveIncluirTodosOsChecksERetornar503ComOracleIndisponivel()
    {
        await using var app = await CriarAplicacaoAsync();

        var response = await app.GetTestClient().GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("self", body);
        Assert.Contains("oracle", body);
    }

    [Fact]
    public async Task Readiness_DeveRetornar200QuandoOracleEstiverAcessivel()
    {
        await using var app = await CriarAplicacaoAsync(oracleDisponivel: true);

        var response = await app.GetTestClient().GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", body);
        Assert.Contains("oracle", body);
    }

    private static async Task<WebApplication> CriarAplicacaoAsync(bool oracleDisponivel = false)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        if (oracleDisponivel)
        {
            builder.Services.AddDbContext<AlertaCidadaoDbContext>(options =>
                options.UseInMemoryDatabase($"health-{Guid.NewGuid():N}"));
        }

        builder.Services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("A API está em execução."),
                tags: new[] { "live" })
            .AddCheck<OracleDatabaseHealthCheck>(
                "oracle",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "database" });

        var app = builder.Build();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        }).AllowAnonymous().DisableRateLimiting();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        }).AllowAnonymous().DisableRateLimiting();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        }).AllowAnonymous().DisableRateLimiting();

        await app.StartAsync();
        return app;
    }
}
