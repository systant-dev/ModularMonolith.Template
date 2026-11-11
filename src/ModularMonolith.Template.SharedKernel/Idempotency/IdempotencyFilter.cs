using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

public sealed class IdempotencyFilter(IIdempotencyStore store, ILogger<IdempotencyFilter> logger) : IEndpointFilter
{
    #region Field Declarations

    public const string IdempotencyKeyHeaderName = "Idempotency-Key";

    public const int MaxKeyLength = 255;

    private const string AnonymousCaller = "anonymous";

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

        if (idempotencyKey.Length > MaxKeyLength)
        {
            return TypedResults.Problem(detail: $"The '{IdempotencyKeyHeaderName}' header must not exceed {MaxKeyLength} characters.",
                                        statusCode: StatusCodes.Status400BadRequest);
        }

        string reservationKey = CreateReservationKey(context.HttpContext, idempotencyKey);
        bool reserved = await store.TryReserveAsync(reservationKey, context.HttpContext.RequestAborted);
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
                await store.ReleaseAsync(reservationKey, context.HttpContext.RequestAborted);
                logger.LogReleasedIdempotencyKey(idempotencyKey);
            }

            return result;
        }
        catch
        {
            await store.ReleaseAsync(reservationKey, CancellationToken.None);
            logger.LogReleasedIdempotencyKey(idempotencyKey);
            throw;
        }
    }

    #endregion

    #region Private Method Declarations

    private static string CreateReservationKey(HttpContext httpContext, string idempotencyKey)
    {
        ClaimsPrincipal user = httpContext.User;
        string caller = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user.FindFirst("sub")?.Value
                        ?? user.Identity?.Name
                        ?? AnonymousCaller;

        return $"{caller}|{httpContext.Request.Method} {httpContext.Request.Path}|{idempotencyKey}";
    }

    #endregion
}
