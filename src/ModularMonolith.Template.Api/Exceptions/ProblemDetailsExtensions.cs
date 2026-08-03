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
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                    Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

                    string? correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationIdItemKey] as string ??
                                            context.HttpContext.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName].FirstOrDefault();

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