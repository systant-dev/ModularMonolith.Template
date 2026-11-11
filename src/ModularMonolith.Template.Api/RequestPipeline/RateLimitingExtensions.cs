using System.Security.Claims;
using System.Threading.RateLimiting;

namespace ModularMonolith.Template.Api.RequestPipeline;

public static class RateLimitingExtensions
{
    #region Field Declarations

    public const string DefaultPolicyName = "default";

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleRateLimiting()
        {
            serviceCollection.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(DefaultPolicyName, httpContext => RateLimitPartition.GetFixedWindowLimiter(GetPartitionKey(httpContext), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromSeconds(10),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
            });

            return serviceCollection;
        }
    }

    #region Private Method Declarations

    private static string GetPartitionKey(HttpContext httpContext)
    {
        ClaimsPrincipal user = httpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            string? subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.Identity.Name;

            if (!string.IsNullOrWhiteSpace(subject))
            {
                return $"user:{subject}";
            }
        }

        return $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }

    #endregion
}
