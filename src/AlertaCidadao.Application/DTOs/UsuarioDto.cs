namespace AlertaCidadao.Application.DTOs;

/// <summary>Representação pública do usuário. Não contém senha nem hash.</summary>
public sealed record UsuarioDto(
    long IdUsuario,
    string Nome,
    string Email,
    string? Telefone,
    string Perfil,
    bool Ativo,
    DateTimeOffset DataCadastro);
