using AlertaCidadao.API.Authentication;
using AlertaCidadao.API.Filters;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Application.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace AlertaCidadao.API.Endpoints;

/// <summary>
/// Define os endpoints de gerenciamento de usuários.
/// </summary>
public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuários");

        // Login: máximo de 5 requisições por minuto por endereço IP.
        grupo.MapPost("/login", async Task<IResult> (
            LoginRequest request,
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null)
                return Results.BadRequest(validacao);

            var email = request.Email.Trim().ToLowerInvariant();
            var usuario = await repository.ObterPorEmailAsync(email, ct);

            // A mesma resposta é usada para e-mail inexistente e senha incorreta.
            if (usuario is null || !usuario.Ativo ||
                !passwordHasher.VerificarSenha(request.Senha, usuario.SenhaHash))
            {
                return Results.Unauthorized();
            }

            var (token, expiraEm) = tokenService.GerarToken(usuario);

            return Results.Ok(new LoginResponse(
                token,
                "Bearer",
                expiraEm,
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil));
        })
        .WithName("Login")
        .WithSummary("Autentica um usuário e retorna um token JWT.")
        .AllowAnonymous()
        .RequireRateLimiting("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests);

        // Cadastro público: máximo de 10 requisições por minuto por endereço IP.
        grupo.MapPost("/", async Task<IResult> (
            CriarUsuarioRequest request,
            IUsuarioService service,
            CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null)
                return Results.BadRequest(validacao);

            var resultado = await service.CriarAsync(request, ct);

            if (!resultado.Sucesso)
                return resultado.ToErrorResult();

            return Results.Created(
                $"/api/usuarios/{resultado.Dados!.IdUsuario}",
                resultado.Dados);
        })
        .AddEndpointFilter<IdempotencyEndpointFilter>()
        .WithName("CriarUsuario")
        .WithSummary("Cadastra um novo cidadão.")
        .WithDescription(
            "O cadastro público cria usuários com perfil CIDADAO. " +
            "A senha é armazenada usando PBKDF2-SHA256.")
        .AllowAnonymous()
        .RequireRateLimiting("Cadastro")
        .Produces<UsuarioDto>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests);

        // Listagem: acesso exclusivo ao perfil GESTOR.
        grupo.MapGet("/", async Task<IResult> (
            IUsuarioService service,
            bool? incluirInativos,
            CancellationToken ct) =>
        {
            return Results.Ok(
                await service.ListarAsync(incluirInativos == true, ct));
        })
        .WithName("ListarUsuarios")
        .WithSummary("Lista os usuários cadastrados.")
        .RequireAuthorization("Gestor")
        .Produces<IReadOnlyList<UsuarioDto>>(StatusCodes.Status200OK);

        // Consulta individual: acesso exclusivo ao perfil GESTOR.
        grupo.MapGet("/{id:long}", async Task<IResult> (
            long id,
            IUsuarioService service,
            CancellationToken ct) =>
        {
            var resultado = await service.ObterPorIdAsync(id, ct);

            return resultado.Sucesso
                ? Results.Ok(resultado.Dados)
                : resultado.ToErrorResult();
        })
        .WithName("ObterUsuarioPorId")
        .WithSummary("Consulta um usuário pelo ID.")
        .RequireAuthorization("Gestor")
        .Produces<UsuarioDto>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // Atualização: acesso exclusivo ao perfil GESTOR.
        grupo.MapPut("/{id:long}", async Task<IResult> (
            long id,
            AtualizarUsuarioRequest request,
            IUsuarioService service,
            CancellationToken ct) =>
        {
            var validacao = ValidacaoRequest.Validar(request);
            if (validacao is not null)
                return Results.BadRequest(validacao);

            var resultado = await service.AtualizarAsync(id, request, ct);

            return resultado.Sucesso
                ? Results.Ok(resultado.Dados)
                : resultado.ToErrorResult();
        })
        .AddEndpointFilter<IdempotencyEndpointFilter>()
        .WithName("AtualizarUsuario")
        .WithSummary("Atualiza um usuário existente.")
        .RequireAuthorization("Gestor")
        .Produces<UsuarioDto>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // Desativação: acesso exclusivo ao perfil GESTOR.
        grupo.MapDelete("/{id:long}", async Task<IResult> (
            long id,
            IUsuarioService service,
            CancellationToken ct) =>
        {
            var resultado = await service.ExcluirAsync(id, ct);

            return resultado.Sucesso
                ? Results.NoContent()
                : resultado.ToErrorResult();
        })
        .AddEndpointFilter<IdempotencyEndpointFilter>()
        .WithName("DesativarUsuario")
        .WithSummary("Desativa um usuário.")
        .RequireAuthorization("Gestor")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
    }
}
