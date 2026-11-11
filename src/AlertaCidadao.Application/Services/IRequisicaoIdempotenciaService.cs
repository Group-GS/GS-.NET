using AlertaCidadao.Application.DTOs;
namespace AlertaCidadao.Application.Services;
public interface IRequisicaoIdempotenciaService
{
    Task<IReadOnlyList<RequisicaoIdempotenciaDto>> ListarAsync(CancellationToken ct = default);
    Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> ObterPorChaveAsync(string chave, CancellationToken ct = default);
    Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> CriarAsync(CriarRequisicaoIdempotenciaRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<RequisicaoIdempotenciaDto>> AtualizarAsync(string chave, AtualizarRequisicaoIdempotenciaRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<bool>> ExcluirAsync(string chave, CancellationToken ct = default);
}
