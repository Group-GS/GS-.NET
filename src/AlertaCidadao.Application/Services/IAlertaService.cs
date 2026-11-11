using AlertaCidadao.Application.DTOs;
namespace AlertaCidadao.Application.Services;
public interface IAlertaService
{
    Task<IReadOnlyList<AlertaDto>> ListarAsync(string? status = null, CancellationToken ct = default);
    Task<ResultadoOperacao<AlertaDto>> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<ResultadoOperacao<AlertaDto>> CriarAsync(CriarAlertaRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<AlertaDto>> AtualizarAsync(long id, AtualizarAlertaRequest request, CancellationToken ct = default);
    Task<ResultadoOperacao<bool>> ExcluirAsync(long id, CancellationToken ct = default);
}
