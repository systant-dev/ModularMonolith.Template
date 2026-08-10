using Microsoft.AspNetCore.Http.Features;
using ModularMonolith.Template.Api.RequestPipeline;
using System.Diagnostics;

namespace ModularMonolith.Template.Api.Exceptions;

public static class ProblemDetailsExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddCustomProblemDetails()
        {
            serviceCollection.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    HttpContext httpContext = context.HttpContext;
                    context.ProblemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", httpContext.TraceIdentifier);

                    Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

                    string? correlationId = httpContext.Items[CorrelationIdMiddleware.CorrelationIdItemKey] as string ??
                                            httpContext.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName].FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(correlationId))
                    {
                        context.ProblemDetails.Extensions.TryAdd(CorrelationIdMiddleware.CorrelationIdItemKey, correlationId);
                    }
                };
            });
            return serviceCollection;
        }
    }
}