using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Auditing;

public sealed class AuditLogFilter(ILogger<AuditLogFilter> logger) : IEndpointFilter
{
    #region Field Declarations

    private const string AnonymousUser = "anonymous";

    #endregion

    #region Public Method Declarations

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        HttpContext httpContext = context.HttpContext;

        try
        {
            object? result = await next(context);
            LogAudit(httpContext, GetStatusCode(result, httpContext));
            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogAudit(httpContext, StatusCodes.Status500InternalServerError);
            throw;
        }
    }

    #endregion

    #region Private Method Declarations

    private static int GetStatusCode(object? result, HttpContext httpContext)
    {
        while (result is INestedHttpResult nested)
        {
            result = nested.Result;
        }

        return result is IStatusCodeHttpResult { StatusCode: int statusCode } ? statusCode : httpContext.Response.StatusCode;
    }

    private void LogAudit(HttpContext httpContext, int statusCode)
    {
        string user = httpContext.User.Identity?.Name
                      ?? httpContext.User.FindFirst("sub")?.Value
                      ?? AnonymousUser;

        logger.LogAuditAction(
            user,
            httpContext.Request.Method,
            httpContext.Request.Path.Value ?? string.Empty,
            statusCode);
    }

    #endregion
}
