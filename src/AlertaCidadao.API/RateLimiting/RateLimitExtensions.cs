using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AlertaCidadao.API.RateLimiting;

/// <summary>
/// Registra e configura as políticas de Rate Limiting da API.
/// </summary>
public static class RateLimitExtensions
{
    /// <summary>
    /// Registra as políticas globais, de login e de cadastro.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(RateLimitSettings.SectionName)
            .Get<RateLimitSettings>() ?? new RateLimitSettings();

        ValidarPolitica("Global", settings.Global);
        ValidarPolitica("Login", settings.Login);
        ValidarPolitica("Cadastro", settings.Cadastro);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            // Política global aplicada aos endpoints.
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    context => CriarParticao(context, settings.Global));

            // Política específica de login.
            options.AddPolicy("Login", context =>
                CriarParticao(context, settings.Login));

            // Política específica de cadastro.
            options.AddPolicy("Cadastro", context =>
                CriarParticao(context, settings.Cadastro));

            // Tratamento de requisições rejeitadas.
            options.OnRejected = async (context, _) =>
            {
                var response = context.HttpContext.Response;

                if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
                {
                    var segundos = Math.Max(
                        1,
                        (int)Math.Ceiling(retryAfter.TotalSeconds));

                    response.Headers["Retry-After"] =
                        segundos.ToString(CultureInfo.InvariantCulture);
                }

                await Results.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Limite de requisições excedido",
                    detail:
                        "Muitas requisições foram recebidas deste endereço IP. " +
                        "Aguarde o tempo informado em Retry-After antes de tentar novamente."
                ).ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    /// <summary>
    /// Cria um limitador individual para o endereço IP do cliente.
    /// </summary>
    private static RateLimitPartition<string> CriarParticao(
        HttpContext context,
        RateLimitPolicySettings policy)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString()
            ?? "ip-desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                QueueLimit = policy.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    }

    /// <summary>
    /// Valida os parâmetros das políticas.
    /// </summary>
    private static void ValidarPolitica(
        string nome,
        RateLimitPolicySettings policy)
    {
        if (policy.PermitLimit <= 0)
        {
            throw new InvalidOperationException(
                $"RateLimiting:{nome}:PermitLimit deve ser maior que zero.");
        }

        if (policy.WindowSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"RateLimiting:{nome}:WindowSeconds deve ser maior que zero.");
        }

        if (policy.QueueLimit < 0)
        {
            throw new InvalidOperationException(
                $"RateLimiting:{nome}:QueueLimit não pode ser negativo.");
        }
    }
}