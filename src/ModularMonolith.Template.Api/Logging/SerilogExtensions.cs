using Microsoft.Extensions.Primitives;
using Serilog;

namespace ModularMonolith.Template.Api.Logging;

public static class SerilogExtensions
{
    extension(WebApplication webApplication)
    {
        public void UseEnrichedSerilogRequestLogging()
        {
            webApplication.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    HttpRequest request = httpContext.Request;

                    diagnosticContext.Set("RequestHost", request.Host.Value);
                    diagnosticContext.Set("RequestScheme", request.Scheme);
                    diagnosticContext.Set("QueryString", request.QueryString.Value);
                    diagnosticContext.Set("ResponseContentType", httpContext.Response.ContentType);

                    if (request.Headers.TryGetValue("User-Agent", out StringValues userAgent))
                    {
                        diagnosticContext.Set("UserAgent", userAgent.ToString());
                    }
                };
            });
        }
    }
}