using AlertaCidadao.Domain.Entities;
namespace AlertaCidadao.Application.Interfaces;
public interface ILeituraEnchenteRepository
{
    Task<IReadOnlyList<LeituraEnchente>> ListarAsync(long? idPonto = null, CancellationToken ct = default);
    Task<LeituraEnchente?> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task<bool> PontoAtivoExisteAsync(long idPonto, CancellationToken ct = default);
    Task<bool> ChaveEventoExisteAsync(string chaveEvento, long? ignorarId = null, CancellationToken ct = default);
    Task<bool> LeituraDoPontoNaDataExisteAsync(long idPonto, DateTimeOffset dataHora, long? ignorarId = null, CancellationToken ct = default);
    Task<bool> PossuiAlertaAsync(long idLeitura, CancellationToken ct = default);
    Task AdicionarAsync(LeituraEnchente entidade, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
    Task RemoverAsync(LeituraEnchente entidade, CancellationToken ct = default);
}
