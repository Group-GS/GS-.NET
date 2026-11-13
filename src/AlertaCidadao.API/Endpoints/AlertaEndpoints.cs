using AlertaCidadao.API.Filters;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
namespace AlertaCidadao.API.Endpoints;
public static class AlertaEndpoints
{
    public static void MapAlertaEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/alertas").WithTags("Alertas");

        grupo.MapGet("/", async Task<IResult> (string? status, IAlertaService service, CancellationToken ct) =>
        {
            if (status?.Length > 20) return ErroValidacao.Campo("status", "O filtro de status deve ter no máximo 20 caracteres.");
            if (!string.IsNullOrWhiteSpace(status) && status.Trim().ToUpperInvariant() is not ("ATIVO" or "RESOLVIDO"))
                return ErroValidacao.Campo("status", "O filtro de status deve ser ATIVO ou RESOLVIDO.");
            return Results.Ok(await service.ListarAsync(status, ct));
        })
            .WithName("ListarAlertas")
            .WithSummary("Lista os alertas cadastrados.")
            .WithDescription("Retorna alertas por data de geração, do mais recente para o mais antigo. O parâmetro status filtra opcionalmente por ATIVO ou RESOLVIDO.")
            .Produces<IReadOnlyList<AlertaDto>>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        grupo.MapGet("/{id:long}", async Task<IResult> (long id, IAlertaService service, CancellationToken ct) =>
        {
            var resultado = await service.ObterPorIdAsync(id, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("ObterAlertaPorId")
            .WithSummary("Consulta um alerta pelo ID.")
            .WithDescription("Retorna classificação, probabilidade, explicação, status e datas do alerta.")
            .Produces<AlertaDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async Task<IResult> (CriarAlertaRequest request, IAlertaService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.CriarAsync(request, ct);
            if (!resultado.Sucesso) return resultado.ToErrorResult();
            return Results.Created($"/api/alertas/{resultado.Dados!.IdAlerta}", resultado.Dados);
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("CriarAlerta")
            .WithSummary("Cria um alerta para uma leitura.")
            .WithDescription("Cria um alerta associado a uma leitura existente. Cada leitura pode ter apenas um alerta associado. Aceita o header opcional Idempotency-Key para impedir a execução duplicada.")
            .Produces<AlertaDto>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:long}", async Task<IResult> (long id, AtualizarAlertaRequest request, IAlertaService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.AtualizarAsync(id, request, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("AtualizarAlerta")
            .WithSummary("Atualiza os dados de um alerta.")
            .WithDescription("Atualiza classificação, probabilidade, mensagem explicativa, modelo e status. Quando o status passa para RESOLVIDO, a data de resolução é registrada.")
            .Produces<AlertaDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapDelete("/{id:long}", async Task<IResult> (long id, IAlertaService service, CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(id, ct);
            return resultado.Sucesso ? Results.NoContent() : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("ExcluirAlerta")
            .WithSummary("Exclui um alerta.")
            .WithDescription("Remove o alerta indicado pelo ID. A operação não remove a leitura de enchente associada.")
            .Produces(StatusCodes.Status204NoContent).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
