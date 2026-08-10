using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.UserModule.Tests.Storage;
using UserModule.Storage;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

/// <summary>
/// Test host that replaces the production Microsoft Entra ID authentication with an
/// always-authenticated test scheme, and swaps the UserModule's SQL Server persistence for a
/// dependency-free in-memory store so integration tests run without a database server. The
/// database initialiser and outbox background processor are removed for the same reason.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    #region Field Declarations

    private const string OutboxProcessorTypeName = "ModularMonolith.Template.SharedKernel.Outbox.OutboxProcessor";

    #endregion

    #region Protected Method Declarations

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            UseInMemoryUserStore(services);
            RemoveDatabaseInitialisers(services);
            RemoveOutboxProcessor(services);
        });
    }

    #endregion

    #region Private Method Declarations

    private static void UseInMemoryUserStore(IServiceCollection services)
    {
        RemoveAll(services, typeof(IUserStore));
        RemoveAll(services, typeof(SqlServerUserStore));

        // Singleton so data persists across the scoped requests of a single test host.
        InMemoryUserStore store = new();
        services.AddSingleton<IUserStore>(store);
    }

    private static void RemoveDatabaseInitialisers(IServiceCollection services) =>
        RemoveAll(services, typeof(IModuleDatabaseInitialiser));

    private static void RemoveOutboxProcessor(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType?.FullName == OutboxProcessorTypeName)
            .ToList())
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveAll(IServiceCollection services, Type serviceType)
    {
        foreach (ServiceDescriptor descriptor in services
            .Where(descriptor => descriptor.ServiceType == serviceType)
            .ToList())
        {
            services.Remove(descriptor);
        }
    }

    #endregion
}
