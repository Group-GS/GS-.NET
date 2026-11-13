using AlertaCidadao.API.Filters;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;
namespace AlertaCidadao.API.Endpoints;
public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/usuarios").WithTags("Usuários");

        grupo.MapGet("/", async Task<IResult> (IUsuarioService service, bool? incluirInativos, CancellationToken ct) =>
            Results.Ok(await service.ListarAsync(incluirInativos == true, ct)))
            .WithName("ListarUsuarios")
            .WithSummary("Lista os usuários cadastrados.")
            .WithDescription("Retorna usuários ativos por padrão. Informe incluirInativos=true para incluir usuários desativados. Senhas e hashes nunca são retornados.")
            .Produces<IReadOnlyList<UsuarioDto>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:long}", async Task<IResult> (long id, IUsuarioService service, CancellationToken ct) =>
        {
            var resultado = await service.ObterPorIdAsync(id, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .WithName("ObterUsuarioPorId")
            .WithSummary("Consulta um usuário pelo ID.")
            .WithDescription("Retorna os dados públicos do usuário. A senha e o hash não fazem parte da resposta.")
            .Produces<UsuarioDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async Task<IResult> (CriarUsuarioRequest request, IUsuarioService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.CriarAsync(request, ct);
            if (!resultado.Sucesso) return resultado.ToErrorResult();
            return Results.Created($"/api/usuarios/{resultado.Dados!.IdUsuario}", resultado.Dados);
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("CriarUsuario")
            .WithSummary("Cadastra um novo usuário.")
            .WithDescription("Valida nome, e-mail, senha, telefone e perfil. A senha é armazenada usando PBKDF2-SHA256 e nunca é devolvida pela API. Aceita o header opcional Idempotency-Key para impedir a execução duplicada.")
            .Produces<UsuarioDto>(StatusCodes.Status201Created).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:long}", async Task<IResult> (long id, AtualizarUsuarioRequest request, IUsuarioService service, CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null) return Results.BadRequest(validacao);
            var resultado = await service.AtualizarAsync(id, request, ct);
            return resultado.Sucesso ? Results.Ok(resultado.Dados) : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("AtualizarUsuario")
            .WithSummary("Atualiza um usuário existente.")
            .WithDescription("Atualiza nome, e-mail, telefone, perfil e situação. A senha é opcional; quando enviada, é gerado um novo hash PBKDF2-SHA256.")
            .Produces<UsuarioDto>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound).Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:long}", async Task<IResult> (long id, IUsuarioService service, CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(id, ct);
            return resultado.Sucesso ? Results.NoContent() : resultado.ToErrorResult();
        })
            .AddEndpointFilter<IdempotencyEndpointFilter>()
            .WithName("DesativarUsuario")
            .WithSummary("Desativa um usuário.")
            .WithDescription("Desativa o usuário sem apagar fisicamente o registro. O histórico permanece preservado.")
            .Produces(StatusCodes.Status204NoContent).Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest).Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
