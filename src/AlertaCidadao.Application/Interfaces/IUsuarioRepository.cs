using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Interfaces;
public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ListarAsync(bool incluirInativos, CancellationToken ct = default);
    Task<Usuario?> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, long? ignorarId = null, CancellationToken ct = default);
    Task AdicionarAsync(Usuario entidade, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
    Task RemoverAsync(Usuario entidade, CancellationToken ct = default);
}
