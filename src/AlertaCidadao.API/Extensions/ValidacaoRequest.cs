using System.ComponentModel.DataAnnotations;
using AlertaCidadao.Application.DTOs;

namespace AlertaCidadao.API.Extensions;

public static class ValidacaoRequest
{
    public static ApiErrorResponse? Validar(object request)
    {
        var contexto = new ValidationContext(request);
        var resultados = new List<ValidationResult>();
        var valido = Validator.TryValidateObject(
            request,
            contexto,
            resultados,
            validateAllProperties: true);

        if (valido)
            return null;

        var erros = resultados
            .SelectMany(resultado => resultado.MemberNames.DefaultIfEmpty("request"),
                (resultado, membro) => new
                {
                    Membro = membro,
                    Mensagem = resultado.ErrorMessage ?? "Valor inválido."
                })
            .GroupBy(x => x.Membro)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(x => x.Mensagem).Distinct().ToArray());

        return new ApiErrorResponse(
            "VALIDACAO",
            "A requisição contém campos inválidos.",
            erros,
            DateTimeOffset.UtcNow);
    }
}
