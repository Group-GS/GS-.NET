using System.Text;
using AlertaCidadao.API.Authentication;
using AlertaCidadao.API.Endpoints;
using AlertaCidadao.API.Extensions;
using AlertaCidadao.Application.Services;
using AlertaCidadao.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AlertaCidadao.API.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oracle")
    ?? throw new InvalidOperationException("A connection string do Oracle não foi configurada.");

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("A seção Jwt não foi configurada.");

if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey) ||
    Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "A chave Jwt:SigningKey deve ser configurada por User Secrets e possuir ao menos 32 bytes.");
}

if (jwtSettings.ExpirationMinutes is < 5 or > 1440)
    throw new InvalidOperationException("Jwt:ExpirationMinutes deve estar entre 5 e 1440 minutos.");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<ITokenService, JwtTokenService>();

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Gestor", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("GESTOR"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlertaCidadao.API",
        Version = "v1",
        Description = "API de monitoramento inteligente de enchentes."
    });
    
    options.OperationFilter<RateLimitOperationFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT. Exemplo: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<OracleExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AlertaCidadao API v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    aplicacao = "AlertaCidadao",
    mensagem = "API de monitoramento de enchentes em execução.",
    documentacao = "/swagger"
}))
.WithName("Inicio")
.WithSummary("Verifica se a API está em execução.")
.WithDescription("Retorna o nome da aplicação, uma mensagem de status e o endereço da documentação Swagger.")
.AllowAnonymous();

app.MapPontoMonitoramentoEndpoints();
app.MapUsuarioEndpoints();
app.MapLeituraEnchenteEndpoints();
app.MapAlertaEndpoints();
app.MapRequisicaoIdempotenciaEndpoints();

app.Run();

public partial class Program { }
