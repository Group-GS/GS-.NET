
using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlertaCidadao.Infrastructure.Repositories;

public sealed class PontoMonitoramentoRepository
    : IPontoMonitoramentoRepository
{
    private readonly AlertaCidadaoDbContext _context;

    public PontoMonitoramentoRepository(
        AlertaCidadaoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PontoMonitoramento>>
        ListarAtivosAsync(
            CancellationToken cancellationToken = default)
    {
        return await _context.PontosMonitoramento
            .AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<PontoMonitoramento?> ObterPorIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PontosMonitoramento
            .FirstOrDefaultAsync(
                p => p.IdPonto == id,
                cancellationToken
            );
    }

    public async Task<bool> CodigoExisteAsync(
        string codigo,
        long? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.PontosMonitoramento
            .AsNoTracking()
            .Where(p => p.Codigo == codigo);

        if (ignorarId.HasValue)
        {
            consulta = consulta.Where(
                p => p.IdPonto != ignorarId.Value
            );
        }

        var quantidade = await consulta.CountAsync(
            cancellationToken
        );

        return quantidade > 0;
    }

    public async Task AdicionarAsync(
        PontoMonitoramento ponto,
        CancellationToken cancellationToken = default)
    {
        await _context.PontosMonitoramento.AddAsync(
            ponto,
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
