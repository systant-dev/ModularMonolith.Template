namespace ModularMonolith.Template.Api.RequestPipeline;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    #region Field Declarations

    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemKey = "correlationId";

    #endregion

    #region Public Method Declarations

    public async Task Invoke(HttpContext context)
    {
        string correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString("n");
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
        context.Items[CorrelationIdItemKey] = correlationId;
        await next(context);
    }

    #endregion
}