using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

public sealed class IdempotencyFilter(IIdempotencyStore store, ILogger<IdempotencyFilter> logger) : IEndpointFilter
{
    #region Field Declarations

    public const string IdempotencyKeyHeaderName = "Idempotency-Key";

    #endregion

    #region Public Method Declarations

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string? idempotencyKey = context.HttpContext.Request.Headers[IdempotencyKeyHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            logger.LogMissingIdempotencyKey(IdempotencyKeyHeaderName);
            return TypedResults.Problem(detail: $"The '{IdempotencyKeyHeaderName}' header is required for this operation.",
                                        statusCode: StatusCodes.Status400BadRequest);
        }

        bool reserved = await store.TryReserveAsync(idempotencyKey, context.HttpContext.RequestAborted);
        if (!reserved)
        {
            logger.LogDuplicateIdempotencyKey(idempotencyKey);
            return TypedResults.Problem(detail: "A request with the same idempotency key has already been processed.",
                                        statusCode: StatusCodes.Status409Conflict);
        }

        try
        {
            object? result = await next(context);

            if (result is IStatusCodeHttpResult { StatusCode: >= StatusCodes.Status500InternalServerError })
            {
                await store.ReleaseAsync(idempotencyKey, context.HttpContext.RequestAborted);
                logger.LogReleasedIdempotencyKey(idempotencyKey);
            }

            return result;
        }
        catch
        {
            await store.ReleaseAsync(idempotencyKey, CancellationToken.None);
            logger.LogReleasedIdempotencyKey(idempotencyKey);
            throw;
        }
    }

    #endregion
}
