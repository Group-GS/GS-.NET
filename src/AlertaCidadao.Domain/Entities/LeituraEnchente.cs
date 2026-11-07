namespace AlertaCidadao.Domain.Entities;

public class LeituraEnchente
{
    public long IdLeitura { get; set; }

    public long IdPonto { get; set; }

    public decimal NivelAguaCm { get; set; }

    public decimal ChuvaMm { get; set; }

    public decimal ElevacaoCmH { get; set; }

    public DateTimeOffset DataHoraLeitura { get; set; }

    public string Origem { get; set; } = "SIMULACAO";

    public string? ChaveEvento { get; set; }

    public DateTimeOffset DataRecebimento { get; set; }

    public PontoMonitoramento Ponto { get; set; } = null!;

    public Alerta? Alerta { get; set; }
}