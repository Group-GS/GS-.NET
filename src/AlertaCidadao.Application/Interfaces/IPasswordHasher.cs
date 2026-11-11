namespace AlertaCidadao.Application.Interfaces;

public interface IPasswordHasher
{
    string GerarHash(string senha);
}
