using AlertaCidadao.Application.DTOs;
namespace AlertaCidadao.API.Extensions;
public static class ErroValidacao
{
    public static IResult Campo(string campo, string mensagem) =>
        Results.Json(new ApiErrorResponse(
            "VALIDACAO",
            "A requisição contém campos inválidos.",
            new Dictionary<string, string[]> { [campo] = [mensagem] },
            DateTimeOffset.UtcNow),
            statusCode: StatusCodes.Status400BadRequest);
}
