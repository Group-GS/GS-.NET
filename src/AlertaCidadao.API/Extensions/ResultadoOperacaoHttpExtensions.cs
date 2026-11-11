using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Application.Services;

namespace AlertaCidadao.API.Extensions;

public static class ResultadoOperacaoHttpExtensions
{
    public static IResult ToErrorResult<T>(this ResultadoOperacao<T> resultado)
    {
        var (status, codigo) = resultado.TipoErro switch
        {
            TipoErroOperacao.Validacao => (StatusCodes.Status400BadRequest, "VALIDACAO"),
            TipoErroOperacao.NaoEncontrado => (StatusCodes.Status404NotFound, "NAO_ENCONTRADO"),
            TipoErroOperacao.Conflito => (StatusCodes.Status409Conflict, "CONFLITO"),
            _ => (StatusCodes.Status500InternalServerError, "ERRO_INTERNO")
        };

        var mensagem = status == StatusCodes.Status500InternalServerError
            ? "Ocorreu um erro interno. Tente novamente ou contate o suporte."
            : resultado.Mensagem ?? "Não foi possível concluir a operação.";

        return Results.Json(
            new ApiErrorResponse(codigo, mensagem, resultado.Erros, DateTimeOffset.UtcNow),
            statusCode: status);
    }
}
