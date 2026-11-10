namespace AlertaCidadao.Application.Services;

public sealed record ResultadoOperacao<T>(
    T? Dados,
    TipoErroOperacao TipoErro,
    string? Mensagem
)
{
    public bool Sucesso => TipoErro == TipoErroOperacao.Nenhum;

    public static ResultadoOperacao<T> Ok(T dados)
    {
        return new ResultadoOperacao<T>(
            dados,
            TipoErroOperacao.Nenhum,
            null
        );
    }

    public static ResultadoOperacao<T> Falhou(
        TipoErroOperacao tipoErro,
        string mensagem
    )
    {
        return new ResultadoOperacao<T>(
            default,
            tipoErro,
            mensagem
        );
    }
}