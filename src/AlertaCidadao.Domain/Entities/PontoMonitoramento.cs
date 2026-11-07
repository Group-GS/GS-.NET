namespace AlertaCidadao.Domain.Entities;

public class PontoMonitoramento
{
    public long IdPonto { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Logradouro { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset DataCadastro { get; set; }

    public ICollection<LeituraEnchente> Leituras { get; set; }
        = new List<LeituraEnchente>();
}