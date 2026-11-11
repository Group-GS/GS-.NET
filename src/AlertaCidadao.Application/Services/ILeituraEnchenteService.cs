using AlertaCidadao.Application.DTOs;
namespace AlertaCidadao.Application.Services;
public interface ILeituraEnchenteService
{
    Task<IReadOnlyList<LeituraEnchenteDto>> ListarAsync(long? idPonto = null, CancellationToken ct = default);
    Task<ResultadoOperacao<LeituraEnchenteDto>> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<ResultadoOperacao<LeituraEnchenteDto>> CriarAsync(CriarLeituraEnchenteRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<LeituraEnchenteDto>> AtualizarAsync(long id, AtualizarLeituraEnchenteRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default);
}
