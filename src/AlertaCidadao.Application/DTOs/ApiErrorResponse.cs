namespace AlertaCidadao.Application.DTOs;

/// <summary>Formato padronizado de erro retornado pela API.</summary>
public sealed record ApiErrorResponse(
    string Codigo,
    string Mensagem,
    IReadOnlyDictionary<string, string[]>? Erros,
    DateTimeOffset DataHora,
    string? TraceId = null);
