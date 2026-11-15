using AlertaCidadao.Application.Services;

namespace AlertaCidadao.UnitTests.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void GerarHash_DeveGerarFormatoPbkdf2Sha256()
    {
        var hash = _hasher.GerarHash("SenhaSegura123!");

        var partes = hash.Split('$');

        Assert.Equal(4, partes.Length);
        Assert.Equal("PBKDF2-SHA256", partes[0]);
        Assert.Equal("600000", partes[1]);
        Assert.Equal(16, Convert.FromBase64String(partes[2]).Length);
        Assert.Equal(32, Convert.FromBase64String(partes[3]).Length);
    }

    [Fact]
    public void GerarHash_DeveGerarSaltDiferenteParaCadaChamada()
    {
        const string senha = "SenhaSegura123!";

        var primeiro = _hasher.GerarHash(senha);
        var segundo = _hasher.GerarHash(senha);

        Assert.NotEqual(primeiro, segundo);
        Assert.NotEqual(primeiro.Split('$')[2], segundo.Split('$')[2]);
    }

    [Fact]
    public void GerarHash_NaoDeveArmazenarSenhaEmTextoClaro()
    {
        const string senha = "SenhaSegura123!";

        var hash = _hasher.GerarHash(senha);

        Assert.DoesNotContain(senha, hash);
    }

    [Fact]
    public void VerificarSenha_DeveAceitarSenhaCorreta()
    {
        var hash = _hasher.GerarHash("SenhaSegura123!");

        Assert.True(_hasher.VerificarSenha("SenhaSegura123!", hash));
    }

    [Fact]
    public void VerificarSenha_DeveRejeitarSenhaIncorreta()
    {
        var hash = _hasher.GerarHash("SenhaSegura123!");

        Assert.False(_hasher.VerificarSenha("OutraSenha123!", hash));
    }

    [Fact]
    public void VerificarSenha_DeveRejeitarHashMalformado()
    {
        Assert.False(_hasher.VerificarSenha("SenhaSegura123!", "hash-invalido"));
    }
}
