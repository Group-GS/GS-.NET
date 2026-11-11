using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados para atualizar os metadados de uma chave de idempotência.</summary>
public sealed record AtualizarRequisicaoIdempotenciaRequest
{
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
