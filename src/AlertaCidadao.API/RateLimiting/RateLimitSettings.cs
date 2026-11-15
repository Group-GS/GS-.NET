namespace AlertaCidadao.API.RateLimiting;

/// <summary>
/// Configurações das políticas de limitação de requisições.
/// </summary>
public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicySettings Global { get; set; } = new()
    {
        PermitLimit = 120,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    public RateLimitPolicySettings Login { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    public RateLimitPolicySettings Cadastro { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60,
        QueueLimit = 0
    };
}

/// <summary>
/// Parâmetros de uma política de Rate Limiting.
/// </summary>
public sealed class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }

    public int QueueLimit { get; set; }
}