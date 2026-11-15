using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlertaCidadao.API.HealthChecks;

/// <summary>
/// Verifica se a API consegue estabelecer conexão com o banco Oracle.
/// </summary>
public sealed class OracleDatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OracleDatabaseHealthCheck> _logger;

    public OracleDatabaseHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<OracleDatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<AlertaCidadaoDbContext>();

            var conectado = await dbContext.Database
                .CanConnectAsync(cancellationToken);

            return conectado
                ? HealthCheckResult.Healthy("Conexão com o Oracle disponível.")
                : HealthCheckResult.Unhealthy("Não foi possível estabelecer conexão com o Oracle.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // A exceção completa fica somente no log; a resposta HTTP não expõe
            // detalhes da conexão, nomes de servidores ou credenciais.
            _logger.LogWarning(exception,
                "Falha ao verificar a conectividade com o banco Oracle.");

            return HealthCheckResult.Unhealthy(
                "Falha ao verificar a conexão com o Oracle.");
        }
    }
}
