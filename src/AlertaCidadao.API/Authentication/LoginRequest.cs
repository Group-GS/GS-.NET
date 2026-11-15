using System.ComponentModel.DataAnnotations;

namespace AlertaCidadao.API.Authentication;

public sealed record LoginRequest
{
    [Required, EmailAddress, StringLength(180)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Senha { get; init; } = string.Empty;
}
