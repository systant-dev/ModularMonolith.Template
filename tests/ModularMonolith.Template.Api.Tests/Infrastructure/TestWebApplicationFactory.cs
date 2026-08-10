using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.SharedKernel.Outbox;
using UserModule.Storage;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

/// <summary>
/// Test host that replaces the production Microsoft Entra ID authentication with
/// a always-authenticated test scheme, and swaps the module databases for the
/// EF Core in-memory provider so integration tests run without SQL Server.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    #region Field Declarations

    private const string OutboxProcessorTypeName = "ModularMonolith.Template.SharedKernel.Outbox.OutboxProcessor";

    private readonly string _databaseName = $"ApiTests-{Guid.NewGuid()}";

    #endregion

    #region Protected Method Declarations

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            RemoveUserDbContextRegistrations(services);
            services.AddModuleDbContext<UserDbContext>((_, options) =>
                options.UseInMemoryDatabase(_databaseName));

            RemoveOutboxProcessor(services);
        });
    }

    #endregion

    #region Private Method Declarations

    private static void RemoveUserDbContextRegistrations(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services
            .Where(descriptor => descriptor.ServiceType == typeof(UserDbContext)
                || (descriptor.ServiceType.IsGenericType
                    && descriptor.ServiceType.GetGenericArguments().Contains(typeof(UserDbContext))))
            .ToList())
        {
            services.Remove(descriptor);
        }
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

    #endregion
}
