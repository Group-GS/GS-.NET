using AlertaCidadao.API.Filters;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
namespace AlertaCidadao.API.Endpoints;
public static class LeituraEnchenteEndpoints
{
    public static void MapLeituraEnchenteEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/leituras-enchente").WithTags("Leituras de Enchente");

        grupo.MapGet("/", async Task<IResult> (long? idPonto, ILeituraEnchenteService service, CancellationToken ct) =>
        {
            if (idPonto is <= 0) return ErroValidacao.Campo("idPonto", "O ID do ponto deve ser maior que zero.");
            return Results.Ok(await service.ListarAsync(idPonto, ct));
        })
            .WithName("ListarLeiturasEnchente")
            .WithSummary("Lista as leituras de enchente.")
            .WithDescription("Retorna as leituras ordenadas da mais recente para a mais antiga. O parâmetro opcional idPonto filtra as leituras de um ponto específico.")
            .Produces<IReadOnlyList<LeituraEnchenteDto>>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        grupo.MapGet("/{id:long}", async Task<IResult> (long id, ILeituraEnchenteService service, CancellationToken ct) =>
        {
            var resultado = await service.ObterPorIdAsync(id, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("ObterLeituraEnchentePorId")
            .WithSummary("Consulta uma leitura pelo ID.")
            .WithDescription("Retorna os valores de nível da água, chuva, elevação, data da medição, origem e ponto associado.")
            .Produces<LeituraEnchenteDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async Task<IResult> (CriarLeituraEnchenteRequest request, ILeituraEnchenteService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.CriarAsync(request, ct);
            if (!resultado.Sucesso) return resultado.ToErrorResult();
            return Results.Created($"/api/leituras-enchente/{resultado.Dados!.IdLeitura}", resultado.Dados);
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("CriarLeituraEnchente")
            .WithSummary("Registra uma nova leitura de enchente.")
            .WithDescription("Registra uma medição vinculada a um ponto ativo. A origem aceita SIMULACAO, SENSOR ou MANUAL; chave de evento e data/hora duplicadas são rejeitadas. Aceita o header opcional Idempotency-Key para impedir a execução duplicada.")
            .Produces<LeituraEnchenteDto>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:long}", async Task<IResult> (long id, AtualizarLeituraEnchenteRequest request, ILeituraEnchenteService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.AtualizarAsync(id, request, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("AtualizarLeituraEnchente")
            .WithSummary("Atualiza uma leitura existente.")
            .WithDescription("Atualiza os dados de medição e sua origem. O ponto e a chave do evento permanecem vinculados ao registro original.")
            .Produces<LeituraEnchenteDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:long}", async Task<IResult> (long id, ILeituraEnchenteService service, CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(id, ct);
            return resultado.Sucesso ? Results.NoContent() : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("ExcluirLeituraEnchente")
            .WithSummary("Exclui uma leitura de enchente.")
            .WithDescription("Remove a leitura quando ela não possui alerta associado. Se houver alerta vinculado, a API retorna conflito para preservar a integridade referencial.")
            .Produces(StatusCodes.Status204NoContent).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
    }
}
