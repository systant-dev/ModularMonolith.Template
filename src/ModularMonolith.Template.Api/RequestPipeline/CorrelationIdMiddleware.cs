using Serilog.Context;
using System.Diagnostics.CodeAnalysis;

namespace ModularMonolith.Template.Api.RequestPipeline;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    #region Field Declarations

    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    public const string CorrelationIdItemKey = "correlationId";

    public const string CorrelationIdLogPropertyName = "CorrelationId";

    public const int MaxCorrelationIdLength = 128;

    #endregion

    #region Public Method Declarations

    public async Task Invoke(HttpContext context)
    {
        string? inboundCorrelationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
        string correlationId = IsAcceptable(inboundCorrelationId) ? inboundCorrelationId : Guid.CreateVersion7().ToString("n");

        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
        context.Items[CorrelationIdItemKey] = correlationId;

        using (LogContext.PushProperty(CorrelationIdLogPropertyName, correlationId))
        {
            await next(context);
        }
    }

    #endregion

    #region Private Method Declarations

    private static bool IsAcceptable([NotNullWhen(true)] string? correlationId)
    {
        return !string.IsNullOrEmpty(correlationId)
               && correlationId.Length <= MaxCorrelationIdLength
               && correlationId.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.');
    }

    #endregion
}
