using AlertaCidadao.Application.Services;

namespace AlertaCidadao.API.Extensions;

public static class ResultadoOperacaoHttpExtensions
{
    public static IResult ToErrorResult<T>(
        this ResultadoOperacao<T> resultado)
    {
        var resposta = new
        {
            mensagem = resultado.Mensagem
        };

        return resultado.TipoErro switch
        {
            TipoErroOperacao.Validacao =>
                Results.BadRequest(resposta),

            TipoErroOperacao.NaoEncontrado =>
                Results.NotFound(resposta),

            TipoErroOperacao.Conflito =>
                Results.Conflict(resposta),

            _ => Results.Problem(
                "Ocorreu um erro inesperado."
            )
        };
    }
}