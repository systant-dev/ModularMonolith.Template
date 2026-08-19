using AccountsModule.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.AccountsModule.Tests.Storage;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.UserModule.Tests.Storage;
using UserModule.Storage;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

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
            UseInMemoryAccountStore(services);
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

    private static void UseInMemoryAccountStore(IServiceCollection services)
    {
        RemoveAll(services, typeof(IAccountStore));
        RemoveAll(services, typeof(IInboxStore));
        RemoveAll(services, typeof(IOutboxStore));
        RemoveAll(services, typeof(SqlServerAccountStore));

        // Singleton so data persists across the scoped requests of a single test host.
        // The in-memory store backs the account, inbox and outbox stores just like the
        // SqlServerAccountStore it replaces, so all three must point at the same instance.
        InMemoryAccountStore store = new();
        services.AddSingleton<IAccountStore>(store);
        services.AddSingleton<IInboxStore>(store);
        services.AddSingleton<IOutboxStore>(store);
    }

    private static void RemoveDatabaseInitialisers(IServiceCollection services)
    {
        RemoveAll(services, typeof(IModuleDatabaseInitialiser));
    }

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
