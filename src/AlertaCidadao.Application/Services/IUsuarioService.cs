using AlertaCidadao.Application.DTOs;
namespace AlertaCidadao.Application.Services;
public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default);
    Task<ResultadoOperacao<UsuarioDto>> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<ResultadoOperacao<UsuarioDto>> CriarAsync(CriarUsuarioRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<UsuarioDto>> AtualizarAsync(long id, AtualizarUsuarioRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default);
}
