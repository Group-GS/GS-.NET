using AlertaCidadao.Application.DTOs;

namespace AlertaCidadao.Application.Services;

public interface IPontoMonitoramentoService
{
    Task<IReadOnlyList<PontoMonitoramentoDto>> ListarAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PontoMonitoramentoDto>> ListarTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<ResultadoOperacao<PontoMonitoramentoDto>> ObterPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<ResultadoOperacao<PontoMonitoramentoDto>> CriarAsync(
        CriarPontoMonitoramentoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ResultadoOperacao<PontoMonitoramentoDto>> AtualizarAsync(
        long id,
        AtualizarPontoMonitoramentoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ResultadoOperacao<bool>> ExcluirAsync(
        long id,
        CancellationToken cancellationToken = default
    );
}