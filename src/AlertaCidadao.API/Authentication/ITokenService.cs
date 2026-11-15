using AlertaCidadao.Domain.Entities;

namespace AlertaCidadao.API.Authentication;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiraEm) GerarToken(Usuario usuario);
}
