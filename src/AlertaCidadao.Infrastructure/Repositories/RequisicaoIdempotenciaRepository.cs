using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AlertaCidadao.Infrastructure.Repositories;
public sealed class RequisicaoIdempotenciaRepository(AlertaCidadaoDbContext context) : IRequisicaoIdempotenciaRepository
{
    public async Task<IReadOnlyList<RequisicaoIdempotencia>> ListarAsync(CancellationToken ct = default) =>
        await context.RequisicoesIdempotencia.AsNoTracking().OrderByDescending(x => x.DataCriacao).ToListAsync(ct);
    public Task<RequisicaoIdempotencia?> ObterPorChaveAsync(string chave, CancellationToken ct = default) =>
        context.RequisicoesIdempotencia.FirstOrDefaultAsync(x => x.ChaveIdempotencia == chave, ct);
    public async Task AdicionarAsync(RequisicaoIdempotencia entidade, CancellationToken ct = default)
    { await context.RequisicoesIdempotencia.AddAsync(entidade, ct); await context.SaveChangesAsync(ct); }
    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
    public async Task RemoverAsync(RequisicaoIdempotencia entidade, CancellationToken ct = default)
    { context.RequisicoesIdempotencia.Remove(entidade); await context.SaveChangesAsync(ct); }
}
