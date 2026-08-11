using Serilog.Context;

namespace ModularMonolith.Template.Api.RequestPipeline;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    #region Field Declarations

    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    public const string CorrelationIdItemKey = "correlationId";

    public const string CorrelationIdLogPropertyName = "CorrelationId";

    #endregion

    #region Public Method Declarations

    public async Task Invoke(HttpContext context)
    {
        string correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault() ?? Guid.CreateVersion7().ToString("n");
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
        context.Items[CorrelationIdItemKey] = correlationId;

        using (LogContext.PushProperty(CorrelationIdLogPropertyName, correlationId))
        {
            await next(context);
        }
    }

    #endregion
}