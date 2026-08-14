using Azure.Monitor.OpenTelemetry.AspNetCore;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.Api.Observability;

public static class ApplicationInsightsExtensions
{
    #region Field Declarations

    private const string ConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    private const string AzureMonitorConnectionStringKey = "AzureMonitor:ConnectionString";

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddApplicationInsightsTelemetry(IConfiguration configuration)
        {
            bool connectionStringConfigured =
                !string.IsNullOrWhiteSpace(configuration[ConnectionStringKey]) ||
                !string.IsNullOrWhiteSpace(configuration[AzureMonitorConnectionStringKey]);

            if (!connectionStringConfigured)
            {
                return serviceCollection;
            }

            serviceCollection.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource(OutboxDiagnostics.Name))
                .WithMetrics(metrics => metrics.AddMeter(OutboxDiagnostics.Name))
                .UseAzureMonitor();

            return serviceCollection;
        }
    }
}
