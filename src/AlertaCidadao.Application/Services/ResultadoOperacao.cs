namespace AlertaCidadao.Application.Services;

public sealed record ResultadoOperacao<T>(
    T? Dados,
    TipoErroOperacao TipoErro,
    string? Mensagem,
    IReadOnlyDictionary<string, string[]>? Erros = null)
{
    public bool Sucesso => TipoErro == TipoErroOperacao.Nenhum;

    public static ResultadoOperacao<T> Ok(T dados) =>
        new(dados, TipoErroOperacao.Nenhum, null);

    public static ResultadoOperacao<T> Falhou(
        TipoErroOperacao tipoErro,
        string mensagem) =>
        new(default, tipoErro, mensagem);

    public static ResultadoOperacao<T> FalhouValidacao(
        string campo,
        string mensagem) =>
        FalhouComCampo(TipoErroOperacao.Validacao, campo, mensagem);

    public static ResultadoOperacao<T> FalhouComCampo(
        TipoErroOperacao tipoErro,
        string campo,
        string mensagem) =>
        new(default, tipoErro, mensagem,
            new Dictionary<string, string[]> { [campo] = [mensagem] });

    public static ResultadoOperacao<T> FalhouComErros(
        TipoErroOperacao tipoErro,
        string mensagem,
        IReadOnlyDictionary<string, string[]> erros) =>
        new(default, tipoErro, mensagem, erros);
}
