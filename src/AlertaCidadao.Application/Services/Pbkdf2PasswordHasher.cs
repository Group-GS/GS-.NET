using System.Security.Cryptography;
using AlertaCidadao.Application.Interfaces;

namespace AlertaCidadao.Application.Services;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iteracoes = 600_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const string Prefixo = "PBKDF2-SHA256";

    public string GerarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"{Prefixo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerificarSenha(string senha, string hashArmazenado)
    {
        if (string.IsNullOrEmpty(senha) || string.IsNullOrWhiteSpace(hashArmazenado))
            return false;

        try
        {
            var partes = hashArmazenado.Split('$');
            if (partes.Length != 4 || partes[0] != Prefixo ||
                !int.TryParse(partes[1], out var iteracoes) ||
                iteracoes < 100_000 || iteracoes > 2_000_000)
                return false;

            var salt = Convert.FromBase64String(partes[2]);
            var hashEsperado = Convert.FromBase64String(partes[3]);
            if (salt.Length != TamanhoSalt || hashEsperado.Length != TamanhoHash)
                return false;

            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                senha, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
