namespace AlertaCidadao.Domain.Entities;

public class Usuario
{
    public long IdUsuario { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    public string Perfil { get; set; } = "CIDADAO";

    public bool Ativo { get; set; } = true;

    public DateTimeOffset DataCadastro { get; set; }
}