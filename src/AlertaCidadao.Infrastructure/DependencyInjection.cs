using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Infrastructure.Persistence;
using AlertaCidadao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace AlertaCidadao.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AlertaCidadaoDbContext>(options => options.UseOracle(connectionString));
        services.AddScoped<IPontoMonitoramentoRepository, PontoMonitoramentoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ILeituraEnchenteRepository, LeituraEnchenteRepository>();
        services.AddScoped<IAlertaRepository, AlertaRepository>();
        services.AddScoped<IRequisicaoIdempotenciaRepository, RequisicaoIdempotenciaRepository>();
        return services;
    }
}
