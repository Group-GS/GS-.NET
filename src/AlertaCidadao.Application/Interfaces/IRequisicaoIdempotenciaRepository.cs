using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Interfaces;
public interface IRequisicaoIdempotenciaRepository
{
    Task<IReadOnlyList<RequisicaoIdempotencia>> ListarAsync(CancellationToken ct = default);
    Task<RequisicaoIdempotencia?> ObterPorChaveAsync(string chave, CancellationToken ct = default);
    Task AdicionarAsync(RequisicaoIdempotencia entidade, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
    Task RemoverAsync(RequisicaoIdempotencia entidade, CancellationToken ct = default);
}
