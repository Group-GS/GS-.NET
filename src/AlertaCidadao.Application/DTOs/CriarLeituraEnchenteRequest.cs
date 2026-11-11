using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados para registrar uma leitura de enchente.</summary>
public sealed record CriarLeituraEnchenteRequest
{
    [Range(1, long.MaxValue, ErrorMessage = "O ID do ponto deve ser maior que zero.")]
    public long IdPonto { get; init; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "O nível da água deve estar entre 0 e 99.999.999,99 cm.")]
    public decimal NivelAguaCm { get; init; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "A chuva deve estar entre 0 e 99.999.999,99 mm.")]
    public decimal ChuvaMm { get; init; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "A elevação deve estar entre 0 e 99.999.999,99 cm/h.")]
    public decimal ElevacaoCmH { get; init; }

    public DateTimeOffset DataHoraLeitura { get; init; }

    [Required(ErrorMessage = "A origem é obrigatória.")]
    [RegularExpression("^(?i:SIMULACAO|SENSOR|MANUAL)$", ErrorMessage = "A origem deve ser SIMULACAO, SENSOR ou MANUAL.")]
    [StringLength(20, ErrorMessage = "A origem deve ter no máximo 20 caracteres.")]
    public string Origem { get; init; } = "SIMULACAO";

    [StringLength(100, ErrorMessage = "A chave do evento deve ter no máximo 100 caracteres.")]
    public string? ChaveEvento { get; init; }
}
