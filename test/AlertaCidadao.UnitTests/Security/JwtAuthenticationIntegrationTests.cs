using System.Text;
using AlertaCidadao.API.Authentication;
using AlertaCidadao.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;

namespace AlertaCidadao.UnitTests.Security;

public sealed class JwtAuthenticationIntegrationTests
{
    private const string Issuer = "AlertaCidadao.API.Tests";
    private const string Audience = "AlertaCidadao.Client.Tests";
    private const string SigningKey = "test-signing-key-with-at-least-32-bytes-123456";

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornar401()
    {
        await using var app = await CriarAplicacaoAsync();
        var response = await app.GetTestClient().GetAsync("/gestor");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointGestor_ComTokenCidadao_DeveRetornar403()
    {
        await using var app = await CriarAplicacaoAsync();
        var token = GerarToken("CIDADAO");
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/gestor");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointGestor_ComTokenGestorValido_DeveRetornar200()
    {
        await using var app = await CriarAplicacaoAsync();
        var token = GerarToken("GESTOR");
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/gestor");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenAlterado_DeveRetornar401()
    {
        await using var app = await CriarAplicacaoAsync();
        var token = GerarToken("GESTOR");
        var partes = token.Split('.');
        partes[2] = (partes[2][0] == 'a' ? "b" : "a") + partes[2][1..];
        var adulterado = string.Join(".", partes);
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adulterado);

        var response = await client.GetAsync("/gestor");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string GerarToken(string perfil)
    {
        var service = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(new JwtSettings
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = SigningKey,
            ExpirationMinutes = 60
        }));

        return service.GerarToken(new Usuario
        {
            IdUsuario = 10,
            Nome = "Usuário de teste",
            Email = "teste@example.com",
            Perfil = perfil,
            Ativo = true
        }).Token;
    }

    private static async Task<WebApplication> CriarAplicacaoAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });
        builder.Services.AddAuthorization(options =>
            options.AddPolicy("Gestor", policy => policy
                .RequireAuthenticatedUser()
                .RequireRole("GESTOR")));

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/gestor", () => Results.Ok(new { acesso = true }))
            .RequireAuthorization("Gestor");
        await app.StartAsync();
        return app;
    }
}
