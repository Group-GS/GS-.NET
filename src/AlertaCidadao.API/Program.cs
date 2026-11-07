using AlertaCidadao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
                           .GetConnectionString("Oracle")
                       ?? throw new InvalidOperationException(
                           "A connection string do Oracle não foi configurada."
                       );

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        aplicacao = "AlertaCidadao",
        mensagem = "API de monitoramento de enchentes em execução."
    });
});

app.Run();

public partial class Program { }