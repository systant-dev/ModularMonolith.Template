using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using ModularMonolith.Template.SharedKernel.Azure;
using System.Diagnostics.CodeAnalysis;

namespace ModularMonolith.Template.SharedKernel.Features;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "False positive on C# extension blocks; the receiver types (IServiceCollection, IApplicationBuilder and ConfigurationManager) are distinct.")]
public static class FeatureManagementExtensions
{
    #region Field Declarations

    private static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromSeconds(30);

    #endregion

    extension(IServiceCollection services)
    {
        public IFeatureManagementBuilder AddModuleFeatureManagement()
        {
            return services.AddFeatureManagement();
        }

        public IServiceCollection AddModuleAppConfiguration()
        {
            services.AddAzureAppConfiguration();
            return services;
        }
    }

    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseModuleAppConfiguration()
        {
            return builder.UseAzureAppConfiguration();
        }
    }

    extension(ConfigurationManager configurationManager)
    {
        public bool ConnectAppConfigurationFeatureFlags(bool isDevelopment,
                                                        string endpointKey = "AppConfiguration:Endpoint",
                                                        string connectionStringKey = "AppConfiguration:ConnectionString",
                                                        bool optional = true)
        {
            string? connectionString = configurationManager[connectionStringKey];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                configurationManager.AddAzureAppConfiguration(options =>
                    options.Connect(connectionString).UseFeatureFlags(featureFlags => featureFlags.SetRefreshInterval(DefaultRefreshInterval)));
                return true;
            }

            string? endpoint = configurationManager[endpointKey];
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                TokenCredential credential = AzureCredentialFactory.CreateDefault(isDevelopment);
                configurationManager.AddAzureAppConfiguration(options =>
                    options.Connect(new Uri(endpoint), credential).UseFeatureFlags(featureFlags => featureFlags.SetRefreshInterval(DefaultRefreshInterval)));
                return true;
            }

            if (!optional)
            {
                throw new InvalidOperationException($"Missing configuration value: '{endpointKey}' or '{connectionStringKey}'");
            }

            return false;
        }
    }
}
