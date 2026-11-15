namespace AlertaCidadao.API.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "AlertaCidadao.API";
    public string Audience { get; init; } = "AlertaCidadao.Client";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
