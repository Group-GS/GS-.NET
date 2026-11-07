namespace AlertaCidadao.Domain.Entities;

public class Alerta
{
    public long IdAlerta { get; set; }

    public long IdLeitura { get; set; }

    public string Classificacao { get; set; } = string.Empty;

    public decimal? Probabilidade { get; set; }

    public string? MensagemExplicativa { get; set; }

    public string? TipoMensagem { get; set; }

    public string? ModeloMl { get; set; }

    public string Status { get; set; } = "ATIVO";

    public DateTimeOffset DataGeracao { get; set; }

    public DateTimeOffset? DataResolucao { get; set; }

    public LeituraEnchente Leitura { get; set; } = null!;
}