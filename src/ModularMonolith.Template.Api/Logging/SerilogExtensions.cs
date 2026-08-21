using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Primitives;
using ModularMonolith.Template.SharedKernel.Compliance;
using Serilog;
using Serilog.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace ModularMonolith.Template.Api.Logging;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "False positive on C# extension blocks; the receiver types (WebApplication and WebApplicationBuilder) are distinct.")]
public static class SerilogExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddSerilogWithRedaction()
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

            builder.Logging.AddSerilog(dispose: true);

            builder.Services.AddSingleton(_ => new DiagnosticContext(Log.Logger));
            builder.Services.AddSingleton<IDiagnosticContext>(serviceProvider => serviceProvider.GetRequiredService<DiagnosticContext>());

            builder.Logging.EnableRedaction();
            builder.Services.AddRedaction(redaction =>
            {
                DataClassificationSet personalDataClassificationSet = new(DataTaxonomy.PersonalData);
                DataClassificationSet sensitiveDataClassificationSet = new(DataTaxonomy.SensitiveData);
                redaction.SetRedactor<MaskingRedactor>(personalDataClassificationSet);
                redaction.SetRedactor<ErasingRedactor>(sensitiveDataClassificationSet);
            });
        }
    }

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
