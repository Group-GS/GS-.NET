using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
namespace AlertaCidadao.API.Endpoints;
public static class PontoMonitoramentoEndpoints
{
    public static void MapPontoMonitoramentoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/pontos-monitoramento").WithTags("Pontos de Monitoramento");

        grupo.MapGet("/", async Task<IResult> (IPontoMonitoramentoService service, CancellationToken ct) =>
            Results.Ok(await service.ListarAsync(ct)))
            .WithName("ListarPontosMonitoramento")
            .WithSummary("Lista os pontos de monitoramento ativos.")
            .WithDescription("Retorna os pontos ativos ordenados pelo nome. Pontos desativados não aparecem nesta listagem.")
            .Produces<IReadOnlyList<PontoMonitoramentoDto>>(StatusCodes.Status200OK);

        grupo.MapGet("/todos", async Task<IResult> (IPontoMonitoramentoService service, CancellationToken ct) =>
            Results.Ok(await service.ListarTodosAsync(ct)))
            .WithName("ListarTodosPontosMonitoramento")
            .WithSummary("Lista todos os pontos de monitoramento.")
            .WithDescription("Retorna pontos ativos e desativados, ordenados pelo nome.")
            .Produces<IReadOnlyList<PontoMonitoramentoDto>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:long}", async Task<IResult> (long id, IPontoMonitoramentoService service, CancellationToken ct) =>
        {
            var resultado = await service.ObterPorIdAsync(id, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("ObterPontoMonitoramentoPorId")
            .WithSummary("Consulta um ponto de monitoramento pelo ID.")
            .WithDescription("Retorna os dados de localização e situação do ponto informado.")
            .Produces<PontoMonitoramentoDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async Task<IResult> (CriarPontoMonitoramentoRequest request, IPontoMonitoramentoService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.CriarAsync(request, ct);
            if (!resultado.Sucesso) return resultado.ToErrorResult();
            return Results.Created($"/api/pontos-monitoramento/{resultado.Dados!.IdPonto}", resultado.Dados);
        })
            .WithName("CriarPontoMonitoramento")
            .WithSummary("Cadastra um novo ponto de monitoramento.")
            .WithDescription("Cadastra um ponto com código único, nome e dados opcionais de endereço e coordenadas. Latitude e longitude devem ser informadas juntas.")
            .Produces<PontoMonitoramentoDto>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:long}", async Task<IResult> (long id, AtualizarPontoMonitoramentoRequest request, IPontoMonitoramentoService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.AtualizarAsync(id, request, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("AtualizarPontoMonitoramento")
            .WithSummary("Atualiza um ponto de monitoramento.")
            .WithDescription("Atualiza código, nome, endereço, coordenadas e situação do ponto informado.")
            .Produces<PontoMonitoramentoDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:long}", async Task<IResult> (long id, IPontoMonitoramentoService service, CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(id, ct);
            return resultado.Sucesso ? Results.NoContent() : resultado.ToErrorResult();
        })
            .WithName("DesativarPontoMonitoramento")
            .WithSummary("Desativa um ponto de monitoramento.")
            .WithDescription("Desativa o ponto sem apagar seu histórico de leituras.")
            .Produces(StatusCodes.Status204NoContent).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
