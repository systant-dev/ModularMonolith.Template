using AccountsModule.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging.Contracts;
using ModularMonolith.Template.SharedKernel.Modular;
using UserModule.Contracts.Events;

namespace ModularMonolith.Template.Api.Tests.Architecture;

public sealed class ModuleContractRegistrationTests
{
    #region Test Method Declarations

    [Fact]
    public void UserModule_OnItsOwn_RegistersEveryEventItPublishes()
    {
        IMessageContractRegistry registry = RegistryFor(new global::UserModule.Module());

        CanResolve<UserCreatedIntegrationEvent>(registry).Should().BeTrue();
        CanResolve<UserUpdatedIntegrationEvent>(registry).Should().BeTrue();
        CanResolve<UserDeletedIntegrationEvent>(registry).Should().BeTrue();
    }

    [Fact]
    public void AccountsModule_OnItsOwn_RegistersEveryEventItPublishes()
    {
        IMessageContractRegistry registry = RegistryFor(new global::AccountsModule.Module());

        CanResolve<AccountOpenedIntegrationEvent>(registry).Should().BeTrue();
    }

    #endregion

    #region Private Method Declarations

    private static IMessageContractRegistry RegistryFor(IModule module)
    {
        ServiceCollection services = new();
        module.RegisterServices(services);
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMessageContractRegistry>();
    }

    private static bool CanResolve<TMessage>(IMessageContractRegistry registry)
    {
        string contractName = MessageContractAttribute.ResolveContractName(typeof(TMessage));
        return registry.TryResolve(contractName, out Type messageType) && messageType == typeof(TMessage);
    }

    #endregion
}
