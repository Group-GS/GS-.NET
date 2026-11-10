using AlertaCidadao.Domain.Entities;

namespace AlertaCidadao.Application.Interfaces;

public interface IPontoMonitoramentoRepository
{
    Task<IReadOnlyList<PontoMonitoramento>> ListarAtivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<PontoMonitoramento?> ObterPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<bool> CodigoExisteAsync(
        string codigo,
        long? ignorarId = null,
        CancellationToken cancellationToken = default
    );

    Task AdicionarAsync(
        PontoMonitoramento ponto,
        CancellationToken cancellationToken = default
    );

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default
    );
}