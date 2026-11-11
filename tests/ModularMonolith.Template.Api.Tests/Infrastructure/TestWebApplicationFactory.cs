using AccountsModule.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.AccountsModule.Tests.Infrastructure.Storage;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.UserModule.Tests.Infrastructure.Storage;
using UserModule.Infrastructure.Storage;

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
        builder.UseSetting("Modules:UserModule:ConnectionString", "Server=localhost;Database=TestUsers;Integrated Security=true");
        builder.UseSetting("Modules:AccountsModule:ConnectionString", "Server=localhost;Database=TestAccounts;Integrated Security=true");
        builder.UseDefaultServiceProvider(options => options.ValidateScopes = true);
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            UseInMemoryUserStore(services);
            UseInMemoryAccountStore(services);
            RemoveDatabaseInitialisers(services);
            RemoveOutboxProcessor(services);
            services.Configure<HealthCheckServiceOptions>(options => options.Registrations.Clear());
        });
    }

    #endregion

    #region Private Method Declarations

    private static void UseInMemoryUserStore(IServiceCollection services)
    {
        RemoveAll(services, typeof(IUserStore));

        InMemoryUserStore store = new();
        services.AddSingleton<IUserStore>(store);
    }

    private static void UseInMemoryAccountStore(IServiceCollection services)
    {
        RemoveAll(services, typeof(IAccountStore));
        RemoveAll(services, typeof(IInboxStore));
        RemoveAll(services, typeof(IOutboxStore));

        InMemoryAccountStore store = new();
        services.AddSingleton<IAccountStore>(store);
        services.AddKeyedSingleton<IInboxStore>(global::AccountsModule.Module.ServiceKey, store);
        services.AddSingleton<IOutboxStore>(store);
    }

    private static void RemoveDatabaseInitialisers(IServiceCollection services)
    {
        RemoveAll(services, typeof(IModuleDatabaseInitialiser));
    }

    private static void RemoveOutboxProcessor(IServiceCollection services)
    {
        List<ServiceDescriptor> serviceDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                                                          && descriptor.ImplementationType?.FullName == OutboxProcessorTypeName)];
        serviceDescriptors.ForEach(descriptor => services.Remove(descriptor));

    }

    private static void RemoveAll(IServiceCollection services, Type serviceType)
    {
        List<ServiceDescriptor> serviceDescriptors = [.. services.Where(descriptor => descriptor.ServiceType == serviceType)];
        serviceDescriptors.ForEach(descriptor => services.Remove(descriptor));
    }

    #endregion
}
