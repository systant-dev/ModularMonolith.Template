using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class MessagingServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddInProcessMessaging_RegistersDispatcherAsSingleton()
    {
        ServiceCollection services = new();

        services.AddInProcessMessaging();

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IMessageDispatcher) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInProcessMessaging_ResolvesDispatcher()
    {
        ServiceCollection services = new();
        services.AddInProcessMessaging();

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<IMessageDispatcher>().Should().NotBeNull();
    }

    [Fact]
    public void AddMessageHandler_RegistersHandlerAsScoped()
    {
        ServiceCollection services = new();

        services.AddMessageHandler<TestMessage, TestHandler>();

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IMessageHandler<TestMessage>) &&
            descriptor.ImplementationType == typeof(TestHandler) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInProcessMessaging_RegistersContractRegistry()
    {
        ServiceCollection services = new();
        services.AddInProcessMessaging();

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<IMessageContractRegistry>().Should().NotBeNull();
    }

    [Fact]
    public void AddMessageHandler_RegistersMessageContract()
    {
        ServiceCollection services = new();
        services.AddMessageHandler<TestMessage, TestHandler>();

        using ServiceProvider provider = services.BuildServiceProvider();
        IMessageContractRegistry registry = provider.GetRequiredService<IMessageContractRegistry>();

        registry.TryResolve(typeof(TestMessage).FullName!, out Type resolved).Should().BeTrue();
        resolved.Should().Be<TestMessage>();
    }

    [Fact]
    public void AddMessageContract_RegistersMessageContract()
    {
        ServiceCollection services = new();
        services.AddMessageContract<TestMessage>();

        using ServiceProvider provider = services.BuildServiceProvider();
        IMessageContractRegistry registry = provider.GetRequiredService<IMessageContractRegistry>();

        registry.TryResolve(typeof(TestMessage).FullName!, out _).Should().BeTrue();
    }

    #endregion

    #region Nested Types

    private sealed record TestMessage : IMessage;

    private sealed class TestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    #endregion
}
