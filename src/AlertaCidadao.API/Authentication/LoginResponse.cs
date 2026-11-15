namespace AlertaCidadao.API.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiraEm,
    long IdUsuario,
    string Nome,
    string Email,
    string Perfil);
