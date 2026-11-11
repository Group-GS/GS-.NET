using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados para atualizar um ponto de monitoramento.</summary>
public sealed record AtualizarPontoMonitoramentoRequest
{
    [Required(ErrorMessage = "O código é obrigatório.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "O código deve ter de 1 a 50 caracteres.")]
    public string Codigo { get; init; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve conter de 2 a 120 caracteres.")]
    public string Nome { get; init; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    public string? Descricao { get; init; }

    [StringLength(180, ErrorMessage = "O logradouro deve ter no máximo 180 caracteres.")]
    public string? Logradouro { get; init; }

    [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
    public string? Bairro { get; init; }

    [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
    public string? Cidade { get; init; }

    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "A UF deve conter exatamente duas letras.")]
    public string? Uf { get; init; }

    [Range(typeof(decimal), "-90", "90", ErrorMessage = "A latitude deve estar entre -90 e 90.")]
    public decimal? Latitude { get; init; }

    [Range(typeof(decimal), "-180", "180", ErrorMessage = "A longitude deve estar entre -180 e 180.")]
    public decimal? Longitude { get; init; }

    public bool Ativo { get; init; } = true;
}
