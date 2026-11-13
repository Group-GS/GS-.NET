using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AlertaCidadao.Application.DTOs;
using AlertaCidadao.Domain.Entities;
using AlertaCidadao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlertaCidadao.API.Filters;

/// <summary>
/// Applies request idempotency to business POST endpoints. The reservation,
/// business operation and stored response share one Oracle transaction.
/// </summary>
public sealed class IdempotencyEndpointFilter(
    AlertaCidadaoDbContext dbContext,
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyEndpointFilter> logger) : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var cancellationToken = httpContext.RequestAborted;

        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var headerValues))
            return await next(context);

        var key = headerValues.ToString().Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
        {
            return Results.BadRequest(CreateError(
                "CHAVE_IDEMPOTENCIA_INVALIDA",
                "O header Idempotency-Key deve conter de 1 a 100 caracteres.",
                httpContext));
        }

        // Minimal API can invoke filters after a binding failure. In that case
        // the handler is skipped and there is no complete request payload to hash.
        if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
            return await next(context);

        var path = httpContext.Request.Path.Value ?? "/";
        if (path.Length > 250)
        {
            return Results.BadRequest(CreateError(
                "ROTA_IDEMPOTENCIA_INVALIDA",
                "A rota da requisição excede o limite permitido para registro de idempotência.",
                httpContext));
        }

        var fingerprint = CalcularFingerprint(context, path);
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existente = await dbContext.RequisicoesIdempotencia
                .SingleOrDefaultAsync(x => x.ChaveIdempotencia == key, cancellationToken);

            if (existente is not null)
            {
                if (existente.DataExpiracao.HasValue && existente.DataExpiracao.Value <= now)
                {
                    dbContext.RequisicoesIdempotencia.Remove(existente);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return CriarRespostaExistente(existente, fingerprint, httpContext);
                }
            }

            var registro = new RequisicaoIdempotencia
            {
                ChaveIdempotencia = key,
                MetodoHttp = httpContext.Request.Method,
                Rota = path,
                FingerprintHash = fingerprint,
                DataCriacao = now,
                DataExpiracao = now.Add(RetentionPeriod)
            };

            dbContext.RequisicoesIdempotencia.Add(registro);
            try
            {
                // The PK insert reserves the key. Concurrent requests using the
                // same key wait on Oracle's unique index until this transaction ends.
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsOracleUniqueViolation(exception))
            {
                await transaction.RollbackAsync(cancellationToken);
                var vencedor = await LerRegistroEmNovoEscopoAsync(key, cancellationToken);
                if (vencedor is null)
                    throw;

                logger.LogInformation(
                    "A requisição concorrente com a chave {IdempotencyKey} foi atendida pelo resultado já persistido.",
                    key);
                return CriarRespostaExistente(vencedor, fingerprint, httpContext);
            }

            var endpointResult = await next(context);
            if (endpointResult is not IResult result)
            {
                throw new InvalidOperationException(
                    "Os endpoints protegidos por idempotência devem retornar IResult.");
            }

            var respostaOriginal = await CapturarRespostaAsync(result, httpContext);
            registro.StatusHttp = respostaOriginal.StatusCode;
            registro.RespostaJson = JsonSerializer.Serialize(respostaOriginal, JsonOptions);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new StoredHttpResult(respostaOriginal);
        }
        catch
        {
            await TentarRollbackAsync(transaction, cancellationToken);
            throw;
        }
    }

    private static string CalcularFingerprint(EndpointFilterInvocationContext context, string path)
    {
        object? payload = null;
        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            var namespaceName = argument.GetType().Namespace ?? string.Empty;
            if (namespaceName.StartsWith("AlertaCidadao.Application.DTOs", StringComparison.Ordinal))
            {
                payload = argument;
                break;
            }
        }

        var payloadJson = payload is null
            ? "null"
            : JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
        var canonicalRequest = $"{context.HttpContext.Request.Method.ToUpperInvariant()}\n{path}\n{payloadJson}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest));
        return Convert.ToHexString(hash);
    }

    private static IResult CriarRespostaExistente(
        RequisicaoIdempotencia existente,
        string fingerprint,
        HttpContext httpContext)
    {
        if (!string.Equals(existente.FingerprintHash, fingerprint, StringComparison.Ordinal))
        {
            return Results.Conflict(CreateError(
                "CHAVE_IDEMPOTENCIA_REUTILIZADA",
                "Esta chave de idempotência já foi utilizada com dados diferentes. Envie uma nova chave para uma nova operação.",
                httpContext));
        }

        if (string.IsNullOrWhiteSpace(existente.RespostaJson))
        {
            return Results.Conflict(CreateError(
                "RESPOSTA_IDEMPOTENCIA_INDISPONIVEL",
                "A chave já está registrada, mas não possui uma resposta original recuperável. A operação não será executada novamente.",
                httpContext));
        }

        try
        {
            var resposta = JsonSerializer.Deserialize<StoredHttpResponse>(existente.RespostaJson, JsonOptions);
            return resposta is null
                ? Results.Conflict(CreateError("RESPOSTA_IDEMPOTENCIA_INVALIDA", "A resposta original armazenada não pôde ser recuperada.", httpContext))
                : new StoredHttpResult(resposta);
        }
        catch (JsonException)
        {
            return Results.Conflict(CreateError(
                "RESPOSTA_IDEMPOTENCIA_INVALIDA",
                "A resposta original armazenada não pôde ser recuperada. A operação não será executada novamente.",
                httpContext));
        }
    }

    private static async Task<StoredHttpResponse> CapturarRespostaAsync(IResult result, HttpContext httpContext)
    {
        var response = httpContext.Response;
        var originalBody = response.Body;
        await using var buffer = new MemoryStream();
        response.Body = buffer;

        try
        {
            await result.ExecuteAsync(httpContext);
            buffer.Position = 0;
            using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var body = await reader.ReadToEndAsync(httpContext.RequestAborted);
            response.Headers.TryGetValue("Location", out var location);

            return new StoredHttpResponse(
                response.StatusCode,
                response.ContentType,
                location.ToString(),
                body);
        }
        finally
        {
            response.Body = originalBody;
        }
    }

    private async Task<RequisicaoIdempotencia?> LerRegistroEmNovoEscopoAsync(
        string key,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<AlertaCidadaoDbContext>();
        return await freshContext.RequisicoesIdempotencia
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ChaveIdempotencia == key, cancellationToken);
    }

    private static bool IsOracleUniqueViolation(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            var numberProperty = current.GetType().GetProperty("Number");
            if (numberProperty?.GetValue(current) is int number && number == 1)
                return true; // ORA-00001: unique constraint violated.
        }

        return false;
    }

    private static async Task TentarRollbackAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        catch
        {
            // Preserve the original exception. The transaction is disposed by the caller.
        }
    }

    private static ApiErrorResponse CreateError(string code, string message, HttpContext context) =>
        new(code, message, null, DateTimeOffset.UtcNow, context.TraceIdentifier);

    private sealed record StoredHttpResponse(
        int StatusCode,
        string? ContentType,
        string? Location,
        string Body);

    private sealed class StoredHttpResult(StoredHttpResponse response) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = response.StatusCode;
            if (!string.IsNullOrWhiteSpace(response.ContentType))
                httpContext.Response.ContentType = response.ContentType;
            if (!string.IsNullOrWhiteSpace(response.Location))
                httpContext.Response.Headers["Location"] = response.Location;

            if (response.Body.Length > 0)
                await httpContext.Response.WriteAsync(response.Body, httpContext.RequestAborted);
        }
    }
}
