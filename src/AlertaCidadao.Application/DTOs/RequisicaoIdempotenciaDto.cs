namespace AlertaCidadao.Application.DTOs;

/// <summary>Metadados de idempotência. O JSON interno da resposta não é exposto.</summary>
public sealed record RequisicaoIdempotenciaDto(
    string ChaveIdempotencia,
    string MetodoHttp,
    string Rota,
    int? StatusHttp,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataExpiracao);
