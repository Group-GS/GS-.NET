using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AlertaCidadao.Infrastructure.Repositories;
public sealed class LeituraEnchenteRepository(AlertaCidadaoDbContext context) : ILeituraEnchenteRepository
{
    public async Task<IReadOnlyList<LeituraEnchente>> ListarAsync(long? idPonto = null, CancellationToken ct = default)
    {
        var query = context.LeiturasEnchente.AsNoTracking();
        if (idPonto.HasValue) query = query.Where(x => x.IdPonto == idPonto.Value);
        return await query.OrderByDescending(x => x.DataHoraLeitura).ThenByDescending(x => x.IdLeitura).ToListAsync(ct);
    }
    public Task<LeituraEnchente?> ObterPorIdAsync(long id, CancellationToken ct = default) =>
        context.LeiturasEnchente.FirstOrDefaultAsync(x => x.IdLeitura == id, ct);
    public async Task<bool> PontoAtivoExisteAsync(long idPonto, CancellationToken ct = default) =>
        await context.PontosMonitoramento.AsNoTracking().Where(x => x.IdPonto == idPonto && x.Ativo).CountAsync(ct) > 0;
    public async Task<bool> ChaveEventoExisteAsync(string chaveEvento, long? ignorarId = null, CancellationToken ct = default)
    {
        var query = context.LeiturasEnchente.AsNoTracking().Where(x => x.ChaveEvento == chaveEvento);
        if (ignorarId.HasValue) query = query.Where(x => x.IdLeitura != ignorarId.Value);
        return await query.CountAsync(ct) > 0;
    }
    public async Task<bool> LeituraDoPontoNaDataExisteAsync(long idPonto, DateTimeOffset dataHora, long? ignorarId = null, CancellationToken ct = default)
    {
        var query = context.LeiturasEnchente.AsNoTracking().Where(x => x.IdPonto == idPonto && x.DataHoraLeitura == dataHora);
        if (ignorarId.HasValue) query = query.Where(x => x.IdLeitura != ignorarId.Value);
        return await query.CountAsync(ct) > 0;
    }
    public async Task<bool> PossuiAlertaAsync(long idLeitura, CancellationToken ct = default) =>
        await context.Alertas.AsNoTracking().Where(x => x.IdLeitura == idLeitura).CountAsync(ct) > 0;
    public async Task AdicionarAsync(LeituraEnchente entidade, CancellationToken ct = default)
    { await context.LeiturasEnchente.AddAsync(entidade, ct); await context.SaveChangesAsync(ct); }
    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
    public async Task RemoverAsync(LeituraEnchente entidade, CancellationToken ct = default)
    { context.LeiturasEnchente.Remove(entidade); await context.SaveChangesAsync(ct); }
}
