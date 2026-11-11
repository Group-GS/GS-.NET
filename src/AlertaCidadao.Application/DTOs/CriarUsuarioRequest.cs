using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.Application.DTOs;

/// <summary>Dados necessários para cadastrar um usuário.</summary>
public sealed record CriarUsuarioRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve conter de 2 a 120 caracteres.")]
    public string Nome { get; init; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
    [StringLength(180, ErrorMessage = "O e-mail deve ter no máximo 180 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve conter pelo menos 8 caracteres.")]
    [StringLength(100, ErrorMessage = "A senha deve ter no máximo 100 caracteres.")]
    public string Senha { get; init; } = string.Empty;

    [Phone(ErrorMessage = "Informe um telefone válido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    public string? Telefone { get; init; }

    [Required(ErrorMessage = "O perfil é obrigatório.")]
    [RegularExpression("^(?i:CIDADAO|GESTOR)$", ErrorMessage = "O perfil deve ser CIDADAO ou GESTOR.")]
    public string Perfil { get; init; } = "CIDADAO";
}
