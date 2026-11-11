using AccountsModule.Infrastructure.Storage;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Storage;
using UserModule.Infrastructure.Storage;

namespace ModularMonolith.Template.Api.Tests.Architecture;

public sealed class ModuleStorageIsolationTests
{
    #region Constant Declarations

    private const string UserDatabase = "UsersDb";
    private const string AccountsDatabase = "AccountsDb";

    #endregion

    #region Test Method Declarations

    [Fact]
    public void EachModule_ResolvesItsOwnConnectionFactory()
    {
        using ServiceProvider provider = BuildModuleServices().BuildServiceProvider();

        ConnectionStringFor(provider, global::UserModule.Module.ServiceKey).Should().Contain(UserDatabase);
        ConnectionStringFor(provider, global::AccountsModule.Module.ServiceKey).Should().Contain(AccountsDatabase);
    }

    [Fact]
    public void Modules_DoNotRegisterAnUnkeyedConnectionFactory()
    {
        ServiceCollection services = BuildModuleServices();

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(IDbConnectionFactory) && !descriptor.IsKeyedService,
                                     "an unkeyed registration is shared by every module, so the last module registered would win");
    }

    [Fact]
    public void ModuleStores_ResolveWithTheirKeyedConnectionFactory()
    {
        using ServiceProvider provider = BuildModuleServices().BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IUserStore>().Should().BeOfType<SqlServerUserStore>();
        scope.ServiceProvider.GetRequiredService<IAccountStore>().Should().BeOfType<SqlServerAccountStore>();
    }

    #endregion

    #region Private Method Declarations

    private static ServiceCollection BuildModuleServices()
    {
        Dictionary<string, string?> settings = new()
        {
            ["Modules:UserModule:MaxUsers"] = "10",
            ["Modules:UserModule:ConnectionString"] = $"Server=localhost;Database={UserDatabase};Integrated Security=true",
            ["Modules:AccountsModule:MaxAccounts"] = "10",
            ["Modules:AccountsModule:ConnectionString"] = $"Server=localhost;Database={AccountsDatabase};Integrated Security=true",
        };

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        ServiceCollection services = new();
        services.AddSingleton(configuration);
        new global::UserModule.Module().RegisterServices(services);
        new global::AccountsModule.Module().RegisterServices(services);
        return services;
    }

    private static string ConnectionStringFor(ServiceProvider provider, string moduleKey)
    {
        IDbConnectionFactory factory = provider.GetRequiredKeyedService<IDbConnectionFactory>(moduleKey);
        return factory.Should().BeOfType<SqlServerConnectionFactory>().Which.ConnectionString;
    }

    #endregion
}
