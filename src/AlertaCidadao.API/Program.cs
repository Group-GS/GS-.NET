using AlertaCidadao.API.Endpoints;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.Services;
using AlertaCidadao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oracle")
                       ?? throw new InvalidOperationException(
                           "A connection string do Oracle não foi configurada.");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(connectionString);

// Configuração dos serviços de documentação Swagger.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "AlertaCidadao.API",
        Version = "v1",
        Description = "API de monitoramento inteligente de enchentes."
    });
});

var app = builder.Build();

app.UseMiddleware<OracleExceptionMiddleware>();

// Disponibiliza o documento JSON do Swagger.
app.UseSwagger();

// Disponibiliza a interface visual Swagger UI.
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "AlertaCidadao API v1");

    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Ok(new
    {
        aplicacao = "AlertaCidadao",
        mensagem = "API de monitoramento de enchentes em execução.",
        documentacao = "/swagger"
    }))
    .WithName("Inicio")
    .WithSummary("Verifica se a API está em execução.")
    .WithDescription(
        "Retorna o nome da aplicação, uma mensagem de status e o endereço da documentação Swagger.");

app.MapPontoMonitoramentoEndpoints();
app.MapUsuarioEndpoints();
app.MapLeituraEnchenteEndpoints();
app.MapAlertaEndpoints();
app.MapRequisicaoIdempotenciaEndpoints();

app.Run();

public partial class Program { }
