namespace AlertaCidadao.Application.DTOs;

public sealed record PontoMonitoramentoDto(
    long IdPonto,
    string Codigo,
    string Nome,
    string? Descricao,
    string? Logradouro,
    string? Bairro,
    string? Cidade,
    string? Uf,
    decimal? Latitude,
    decimal? Longitude,
    bool Ativo,
    DateTimeOffset DataCadastro
);