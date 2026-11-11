using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularMonolith.Template.SharedKernel.Storage;

namespace ModularMonolith.Template.SharedKernel.HealthChecks;

public static class DatabaseHealthCheckExtensions
{
    #region Field Declarations

    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    #endregion

    extension(IHealthChecksBuilder healthChecks)
    {
        public IHealthChecksBuilder AddModuleDatabaseCheck(string connectionFactoryKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionFactoryKey);

            HealthCheckRegistration registration = new(
                $"{connectionFactoryKey}-database",
                provider => new DbConnectionHealthCheck(provider.GetRequiredKeyedService<IDbConnectionFactory>(connectionFactoryKey)),
                HealthStatus.Unhealthy,
                [HealthCheckTags.Ready],
                CheckTimeout);

            return healthChecks.Add(registration);
        }
    }
}
