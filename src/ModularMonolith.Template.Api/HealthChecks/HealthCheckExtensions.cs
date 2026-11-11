using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ModularMonolith.Template.SharedKernel.HealthChecks;

namespace ModularMonolith.Template.Api.HealthChecks;

public static class HealthCheckExtensions
{
    #region Constant Declarations

    public const string LiveTag = HealthCheckTags.Live;

    public const string ReadyTag = HealthCheckTags.Ready;

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
