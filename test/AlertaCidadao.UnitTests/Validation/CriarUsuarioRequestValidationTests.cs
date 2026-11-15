using System.ComponentModel.DataAnnotations;
using AlertaCidadao.Application.DTOs;

namespace AlertaCidadao.UnitTests.Validation;

public sealed class CriarUsuarioRequestValidationTests
{
    private static IList<ValidationResult> Validar(CriarUsuarioRequest request)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            resultados,
            validateAllProperties: true);
        return resultados;
    }

    [Fact]
    public void Request_ComDadosValidos_DevePassarValidacao()
    {
        var request = new CriarUsuarioRequest
        {
            Nome = "Maria da Silva",
            Email = "maria@example.com",
            Senha = "SenhaSegura123!",
            Perfil = "CIDADAO"
        };

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void Request_ComEmailInvalido_DeveFalharValidacao()
    {
        var request = new CriarUsuarioRequest
        {
            Nome = "Maria da Silva",
            Email = "email-invalido",
            Senha = "SenhaSegura123!",
            Perfil = "CIDADAO"
        };

        Assert.Contains(Validar(request), x =>
            x.MemberNames.Contains(nameof(CriarUsuarioRequest.Email)));
    }

    [Fact]
    public void Request_ComSenhaCurta_DeveFalharValidacao()
    {
        var request = new CriarUsuarioRequest
        {
            Nome = "Maria da Silva",
            Email = "maria@example.com",
            Senha = "123",
            Perfil = "CIDADAO"
        };

        Assert.Contains(Validar(request), x =>
            x.MemberNames.Contains(nameof(CriarUsuarioRequest.Senha)));
    }

    [Fact]
    public void UsuarioDto_NaoDeveExporSenhaOuHash()
    {
        var propriedades = typeof(UsuarioDto).GetProperties()
            .Select(x => x.Name)
            .ToArray();

        Assert.DoesNotContain("Senha", propriedades);
        Assert.DoesNotContain("SenhaHash", propriedades);
        Assert.DoesNotContain("Password", propriedades);
    }
}
