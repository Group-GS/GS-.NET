using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Interfaces;
public interface IAlertaRepository
{
    Task<IReadOnlyList<Alerta>> ListarAsync(string? status = null, CancellationToken ct = default);
    Task<Alerta?> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<bool> LeituraExisteAsync(long idLeitura, CancellationToken ct = default);
    Task<bool> AlertaParaLeituraExisteAsync(long idLeitura, long? ignorarId = null, CancellationToken ct = default);
    Task AdicionarAsync(Alerta entidade, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
    Task RemoverAsync(Alerta entidade, CancellationToken ct = default);
}
