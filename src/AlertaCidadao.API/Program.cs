using AlertaCidadao.API.Endpoints;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.Services;
using AlertaCidadao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oracle")
    ?? throw new InvalidOperationException("A connection string do Oracle não foi configurada.");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(connectionString);

// Documentação OpenAPI nativa do ASP.NET Core. Não existe integração com OpenAI.
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseMiddleware<OracleExceptionMiddleware>();

// Documento JSON com Summary e Description de cada endpoint.
app.MapOpenApi();

app.MapGet("/", () => Results.Ok(new
{
    aplicacao = "AlertaCidadao",
    mensagem = "API de monitoramento de enchentes em execução.",
    documentacao = "/openapi/v1.json"
}))
.WithName("Inicio")
.WithSummary("Verifica se a API está em execução.")
.WithDescription("Retorna o nome da aplicação, uma mensagem de status e o endereço do documento OpenAPI.");

app.MapPontoMonitoramentoEndpoints();
app.MapUsuarioEndpoints();
app.MapLeituraEnchenteEndpoints();
app.MapAlertaEndpoints();
app.MapRequisicaoIdempotenciaEndpoints();

app.Run();

public partial class Program { }
