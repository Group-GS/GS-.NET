using System.Security.Cryptography;
using AlertaCidadao.Application.Interfaces;

namespace AlertaCidadao.Application.Services;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iteracoes = 600_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public string GerarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"PBKDF2-SHA256${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}
