using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
namespace AlertaCidadao.API.Endpoints;
public static class RequisicaoIdempotenciaEndpoints
{
    public static void MapRequisicaoIdempotenciaEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/requisicoes-idempotencia")
            .WithTags("Requisições de Idempotência")
            .RequireAuthorization("Gestor");

        grupo.MapGet("/", async Task<IResult> (IRequisicaoIdempotenciaService service, CancellationToken ct) =>
            Results.Ok(await service.ListarAsync(ct)))
            .WithName("ListarRequisicoesIdempotencia")
            .WithSummary("Lista registros de idempotência.")
            .WithDescription("Retorna os metadados das chaves registradas. O conteúdo interno de RespostaJson não é exposto.")
            .Produces<IReadOnlyList<RequisicaoIdempotenciaDto>>(StatusCodes.Status200OK);

        grupo.MapGet("/{chave}", async Task<IResult> (string chave, IRequisicaoIdempotenciaService service, CancellationToken ct) =>
        {
            var resultado = await service.ObterPorChaveAsync(chave, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("ObterRequisicaoIdempotenciaPorChave")
            .WithSummary("Consulta um registro pela chave de idempotência.")
            .WithDescription("Busca os metadados da chave. O JSON armazenado internamente nunca é retornado pela API.")
            .Produces<RequisicaoIdempotenciaDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async Task<IResult> (CriarRequisicaoIdempotenciaRequest request, IRequisicaoIdempotenciaService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.CriarAsync(request, ct);
            if (!resultado.Sucesso) return resultado.ToErrorResult();
            return Results.Created($"/api/requisicoes-idempotencia/{Uri.EscapeDataString(resultado.Dados!.ChaveIdempotencia)}", resultado.Dados);
        })
            .WithName("CriarRequisicaoIdempotencia")
            .WithSummary("Cria um registro de idempotência.")
            .WithDescription("Cadastra uma chave única, método HTTP, rota, status HTTP e expiração. RespostaJson é um dado interno e não pode ser informado por este endpoint.")
            .Produces<RequisicaoIdempotenciaDto>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapPut("/{chave}", async Task<IResult> (string chave, AtualizarRequisicaoIdempotenciaRequest request, IRequisicaoIdempotenciaService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.AtualizarAsync(chave, request, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("AtualizarRequisicaoIdempotencia")
            .WithSummary("Atualiza os metadados de um registro de idempotência.")
            .WithDescription("Atualiza método HTTP, rota, status HTTP e data de expiração. A chave permanece imutável e RespostaJson não é exposto.")
            .Produces<RequisicaoIdempotenciaDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapDelete("/{chave}", async Task<IResult> (string chave, IRequisicaoIdempotenciaService service, CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(chave, ct);
            return resultado.Sucesso ? Results.NoContent() : resultado.ToErrorResult();
        })
            .WithName("ExcluirRequisicaoIdempotencia")
            .WithSummary("Exclui um registro de idempotência.")
            .WithDescription("Remove o registro da chave informada. Use esta operação com cuidado, pois registros de idempotência ajudam a evitar o processamento duplicado de requisições.")
            .Produces(StatusCodes.Status204NoContent).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
