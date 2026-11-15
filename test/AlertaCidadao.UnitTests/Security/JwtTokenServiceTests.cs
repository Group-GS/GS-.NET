using System.IdentityModel.Tokens.Jwt;
using AlertaCidadao.API.Authentication;
using AlertaCidadao.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AlertaCidadao.UnitTests.Security;

public sealed class JwtTokenServiceTests
{
    private static JwtTokenService CriarServico() =>
        new(Options.Create(new JwtSettings
        {
            Issuer = "AlertaCidadao.API",
            Audience = "AlertaCidadao.Client",
            SigningKey = "chave-de-teste-com-mais-de-32-bytes-123456",
            ExpirationMinutes = 60
        }));

    [Fact]
    public void GerarToken_DeveIncluirIdentidadeEmailEPerfil()
    {
        var usuario = new Usuario
        {
            IdUsuario = 42,
            Nome = "Usuário Teste",
            Email = "teste@exemplo.com",
            Perfil = "GESTOR",
            Ativo = true
        };

        var (tokenString, expiraEm) = CriarServico().GerarToken(usuario);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        Assert.Equal("AlertaCidadao.API", token.Issuer);
        Assert.Contains("AlertaCidadao.Client", token.Audiences);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "42");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "teste@exemplo.com");
        Assert.Contains(token.Claims, c => c.Type == "role" && c.Value == "GESTOR");
        Assert.True(expiraEm > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GerarToken_DeveDefinirExpiracaoConfigurada()
    {
        var usuario = new Usuario
        {
            IdUsuario = 7,
            Nome = "Cidadão",
            Email = "cidadao@exemplo.com",
            Perfil = "CIDADAO"
        };

        var antes = DateTimeOffset.UtcNow;
        var (_, expiraEm) = CriarServico().GerarToken(usuario);

        Assert.InRange(expiraEm, antes.AddMinutes(59), DateTimeOffset.UtcNow.AddMinutes(61));
    }
}
