namespace AlertaCidadao.Domain.Entities;

public class RequisicaoIdempotencia
{
    public string ChaveIdempotencia { get; set; } = string.Empty;

    public string MetodoHttp { get; set; } = string.Empty;

    public string Rota { get; set; } = string.Empty;

    /// <summary>Hash SHA-256 do método, rota e conteúdo da requisição original.</summary>
    public string? FingerprintHash { get; set; }

    public int? StatusHttp { get; set; }

    public string? RespostaJson { get; set; }

    public DateTimeOffset DataCriacao { get; set; }

    public DateTimeOffset? DataExpiracao { get; set; }
}