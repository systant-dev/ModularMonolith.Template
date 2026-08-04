using Microsoft.AspNetCore.Http.Timeouts;

namespace ModularMonolith.Template.Api.RequestPipeline;

public static class RequestTimeoutExtensions
{
    #region Field Declarations

    public const string DefaultPolicyName = "default";

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleRequestTimeouts()
        {
            serviceCollection.AddRequestTimeouts(options =>
            {
                options.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                };
                options.AddPolicy(DefaultPolicyName, TimeSpan.FromSeconds(30));
            });

            return serviceCollection;
        }
    }
}