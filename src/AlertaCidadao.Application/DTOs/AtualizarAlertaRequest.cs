using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados para atualizar os dados e o status de um alerta.</summary>
public sealed record AtualizarAlertaRequest
{
    [Required(ErrorMessage = "A classificação é obrigatória.")]
    [RegularExpression("^(?i:BAIXO|MODERADO|ALTO|CRITICO)$", ErrorMessage = "A classificação deve ser BAIXO, MODERADO, ALTO ou CRITICO.")]
    [StringLength(20, ErrorMessage = "A classificação deve ter no máximo 20 caracteres.")]
    public string Classificacao { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "1", ErrorMessage = "A probabilidade deve estar entre 0 e 1.")]
    public decimal? Probabilidade { get; init; }

    public string? MensagemExplicativa { get; init; }

    [StringLength(20, ErrorMessage = "O tipo da mensagem deve ter no máximo 20 caracteres.")]
    public string? TipoMensagem { get; init; }

    [StringLength(100, ErrorMessage = "O nome do modelo deve ter no máximo 100 caracteres.")]
    public string? ModeloMl { get; init; }

    [Required(ErrorMessage = "O status é obrigatório.")]
    [RegularExpression("^(?i:ATIVO|RESOLVIDO)$", ErrorMessage = "O status deve ser ATIVO ou RESOLVIDO.")]
    public string Status { get; init; } = "ATIVO";
}
