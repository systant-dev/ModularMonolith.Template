using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ModularMonolith.Template.Api.HealthChecks;

public static class HealthCheckExtensions
{
    #region Field Declarations

    public const string LiveTag = "live";

    public const string ReadyTag = "ready";

    #endregion

    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapModuleHealthChecks()
        {
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(LiveTag) || registration.Tags.Count == 0,
            });

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(ReadyTag),
            });

            return endpoints;
        }
    }
}