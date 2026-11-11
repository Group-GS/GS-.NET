using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AlertaCidadao.Infrastructure.Repositories;
public sealed class AlertaRepository(AlertaCidadaoDbContext context) : IAlertaRepository
{
    public async Task<IReadOnlyList<Alerta>> ListarAsync(string? status = null, CancellationToken ct = default)
    {
        var query = context.Alertas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        return await query.OrderByDescending(x => x.DataGeracao).ThenByDescending(x => x.IdAlerta).ToListAsync(ct);
    }
    public Task<Alerta?> ObterPorIdAsync(long id, CancellationToken ct = default) =>
        context.Alertas.FirstOrDefaultAsync(x => x.IdAlerta == id, ct);
    public async Task<bool> LeituraExisteAsync(long idLeitura, CancellationToken ct = default) =>
        await context.LeiturasEnchente.AsNoTracking().Where(x => x.IdLeitura == idLeitura).CountAsync(ct) > 0;
    public async Task<bool> AlertaParaLeituraExisteAsync(long idLeitura, long? ignorarId = null, CancellationToken ct = default)
    {
        var query = context.Alertas.AsNoTracking().Where(x => x.IdLeitura == idLeitura);
        if (ignorarId.HasValue) query = query.Where(x => x.IdAlerta != ignorarId.Value);
        return await query.CountAsync(ct) > 0;
    }
    public async Task AdicionarAsync(Alerta entidade, CancellationToken ct = default)
    { await context.Alertas.AddAsync(entidade, ct); await context.SaveChangesAsync(ct); }
    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
    public async Task RemoverAsync(Alerta entidade, CancellationToken ct = default)
    { context.Alertas.Remove(entidade); await context.SaveChangesAsync(ct); }
}
