using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlertaCidadao.API.RateLimiting;

/// <summary>
/// Adiciona a resposta HTTP 429 à documentação dos endpoints.
/// </summary>
public sealed class RateLimitOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        operation.Responses.TryAdd(
            "429",
            new OpenApiResponse
            {
                Description = "Limite de requisições excedido."
            });
    }
}