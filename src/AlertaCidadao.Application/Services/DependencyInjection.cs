using Microsoft.Extensions.DependencyInjection;

namespace AlertaCidadao.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<
            IPontoMonitoramentoService,
            PontoMonitoramentoService
        >();

        return services;
    }
}