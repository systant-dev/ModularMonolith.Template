using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ModularMonolith.Template.Api.HealthChecks;

public static class HealthCheckExtensions
{
    #region Constant Declarations

    public const string LiveTag = "live";

    public const string ReadyTag = "ready";

    #endregion

    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapModuleHealthChecks()
        {
            HealthCheckOptions liveOptions = new()
            {
                Predicate = registration => registration.Tags.Contains(LiveTag) || registration.Tags.Count == 0,
            };

            endpoints.MapHealthChecks("/health/live", liveOptions).AllowAnonymous();

            HealthCheckOptions readyOptions = new()
            {
                Predicate = registration => registration.Tags.Contains(ReadyTag),
            };

            endpoints.MapHealthChecks("/health/ready", readyOptions).AllowAnonymous();

            return endpoints;
        }
    }
}
