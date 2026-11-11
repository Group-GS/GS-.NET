using AlertaCidadao.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace AlertaCidadao.Application.Services;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPontoMonitoramentoService, PontoMonitoramentoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ILeituraEnchenteService, LeituraEnchenteService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IRequisicaoIdempotenciaService, RequisicaoIdempotenciaService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        return services;
    }
}
