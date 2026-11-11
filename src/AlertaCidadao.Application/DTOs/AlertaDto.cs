namespace AlertaCidadao.Application.DTOs;

public sealed record AlertaDto(
    long IdAlerta,
    long IdLeitura,
    string Classificacao,
    decimal? Probabilidade,
    string? MensagemExplicativa,
    string? TipoMensagem,
    string? ModeloMl,
    string Status,
    DateTimeOffset DataGeracao,
    DateTimeOffset? DataResolucao);
