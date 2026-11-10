using AlertaCidadao.Application.Interfaces;
using AlertaCidadao.Infrastructure.Persistence;
using AlertaCidadao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaCidadao.Infrastructure;

public static class DependencyInjection
{
    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection
        AddInfrastructure(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            string connectionString)
    {
        services.AddDbContext<AlertaCidadaoDbContext>(options =>
        {
            options.UseOracle(connectionString);
        });

        services.AddScoped<
            IPontoMonitoramentoRepository,
            PontoMonitoramentoRepository
        >();

        return services;
    }
}