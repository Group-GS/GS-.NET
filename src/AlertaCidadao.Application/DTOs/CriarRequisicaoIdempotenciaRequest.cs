using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados para cadastrar metadados de uma chave de idempotência.</summary>
public sealed record CriarRequisicaoIdempotenciaRequest
{
    [Required(ErrorMessage = "A chave de idempotência é obrigatória.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "A chave deve ter de 1 a 100 caracteres.")]
    public string ChaveIdempotencia { get; init; } = string.Empty;

    [Required(ErrorMessage = "O método HTTP é obrigatório.")]
    [RegularExpression("^(?i:GET|POST|PUT|PATCH|DELETE)$", ErrorMessage = "O método HTTP deve ser GET, POST, PUT, PATCH ou DELETE.")]
    [StringLength(10, ErrorMessage = "O método HTTP deve ter no máximo 10 caracteres.")]
    public string MetodoHttp { get; init; } = string.Empty;

    [Required(ErrorMessage = "A rota é obrigatória.")]
    [StringLength(250, ErrorMessage = "A rota deve ter no máximo 250 caracteres.")]
    public string Rota { get; init; } = string.Empty;

    [Range(100, 599, ErrorMessage = "O status HTTP deve estar entre 100 e 599.")]
    public int? StatusHttp { get; init; }

    public DateTimeOffset? DataExpiracao { get; init; }
}
