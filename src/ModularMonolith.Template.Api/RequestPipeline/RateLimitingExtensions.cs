using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

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
                options.AddFixedWindowLimiter(DefaultPolicyName, limiter =>
                {
                    limiter.PermitLimit = 100;
                    limiter.Window = TimeSpan.FromSeconds(10);
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 0;
                });
            });

            return serviceCollection;
        }
    }
}