using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AlertaCidadao.Infrastructure.Repositories;
public sealed class UsuarioRepository(AlertaCidadaoDbContext context) : IUsuarioRepository
{
    public async Task<IReadOnlyList<Usuario>> ListarAsync(bool incluirInativos, CancellationToken ct = default)
    {
        var query = context.Usuarios.AsNoTracking();
        if (!incluirInativos) query = query.Where(x => x.Ativo);
        return await query.OrderBy(x => x.Nome).ThenBy(x => x.IdUsuario).ToListAsync(ct);
    }
    public Task<Usuario?> ObterPorIdAsync(long id, CancellationToken ct = default) =>
        context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, ct);
    public async Task<bool> EmailExisteAsync(string email, long? ignorarId = null, CancellationToken ct = default)
    {
        var query = context.Usuarios.AsNoTracking().Where(x => x.Email == email);
        if (ignorarId.HasValue) query = query.Where(x => x.IdUsuario != ignorarId.Value);
        return await query.CountAsync(ct) > 0;
    }
    public async Task AdicionarAsync(Usuario entidade, CancellationToken ct = default)
    { await context.Usuarios.AddAsync(entidade, ct); await context.SaveChangesAsync(ct); }
    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
    public async Task RemoverAsync(Usuario entidade, CancellationToken ct = default)
    { context.Usuarios.Remove(entidade); await context.SaveChangesAsync(ct); }
}
