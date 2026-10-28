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

        object? result = await next(context);

        string user = httpContext.User.Identity?.Name
                      ?? httpContext.User.FindFirst("sub")?.Value
                      ?? AnonymousUser;

        logger.LogAuditAction(
            user,
            httpContext.Request.Method,
            httpContext.Request.Path.Value ?? string.Empty,
            httpContext.Response.StatusCode);

        return result;
    }

    #endregion
}
