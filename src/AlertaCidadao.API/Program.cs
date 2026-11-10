using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
using AlertaCidadao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("Oracle")
    ?? throw new InvalidOperationException(
        "A connection string do Oracle não foi configurada."
    );

// Registro das camadas
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(connectionString);

// Documentação OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Rota principal
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        aplicacao = "AlertaCidadao",
        mensagem = "API de monitoramento de enchentes em execução."
    });
})
.WithName("Inicio")
.WithSummary("Verifica se a API está funcionando.");

// Grupo de endpoints de pontos de monitoramento
var pontos = app.MapGroup("/api/pontos-monitoramento")
    .WithTags("Pontos de Monitoramento");

// GET: listar pontos ativos
pontos.MapGet("/", async (
    IPontoMonitoramentoService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.ListarAsync(cancellationToken);

    return Results.Ok(resultado);
})
.WithName("ListarPontosMonitoramento")
.WithSummary("Lista os pontos de monitoramento ativos.");

// GET: consultar ponto por ID
pontos.MapGet("/{id:long}", async (
    long id,
    IPontoMonitoramentoService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.ObterPorIdAsync(
        id,
        cancellationToken
    );

    if (!resultado.Sucesso)
        return resultado.ToErrorResult();

    return Results.Ok(resultado.Dados);
})
.WithName("ObterPontoMonitoramentoPorId")
.WithSummary("Consulta um ponto de monitoramento pelo ID.");

// POST: cadastrar ponto
pontos.MapPost("/", async (
    CriarPontoMonitoramentoRequest request,
    IPontoMonitoramentoService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.CriarAsync(
        request,
        cancellationToken
    );

    if (!resultado.Sucesso)
        return resultado.ToErrorResult();

    var ponto = resultado.Dados!;

    return Results.Created(
        $"/api/pontos-monitoramento/{ponto.IdPonto}",
        ponto
    );
})
.WithName("CriarPontoMonitoramento")
.WithSummary("Cadastra um novo ponto de monitoramento.");

// PUT: atualizar ponto
pontos.MapPut("/{id:long}", async (
    long id,
    AtualizarPontoMonitoramentoRequest request,
    IPontoMonitoramentoService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.AtualizarAsync(
        id,
        request,
        cancellationToken
    );

    if (!resultado.Sucesso)
        return resultado.ToErrorResult();

    return Results.Ok(resultado.Dados);
})
.WithName("AtualizarPontoMonitoramento")
.WithSummary("Atualiza um ponto de monitoramento.");

// DELETE: desativar ponto
pontos.MapDelete("/{id:long}", async (
    long id,
    IPontoMonitoramentoService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.ExcluirAsync(
        id,
        cancellationToken
    );

    if (!resultado.Sucesso)
        return resultado.ToErrorResult();

    return Results.NoContent();
})
.WithName("ExcluirPontoMonitoramento")
.WithSummary("Desativa um ponto de monitoramento.");

app.Run();

public partial class Program { }
