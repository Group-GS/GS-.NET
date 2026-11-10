namespace AlertaCidadao.Application.DTOs;

public sealed record CriarPontoMonitoramentoRequest
{
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public string? Logradouro { get; init; }
    public string? Bairro { get; init; }
    public string? Cidade { get; init; }
    public string? Uf { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}