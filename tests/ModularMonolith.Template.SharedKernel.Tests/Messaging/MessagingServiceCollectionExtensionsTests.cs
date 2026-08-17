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
        services.AddSingleton<IMessageDispatchMap, TestDispatchMap>();

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<IMessageDispatcher>().Should().NotBeNull();
    }

    [Fact]
    public void AddMessageHandler_RegistersHandlerAsScoped()
    {
        ServiceCollection services = new();

        services.AddMessageHandler<TestMessage, TestHandler>();

        // The concrete handler is registered as scoped and IMessageHandler<T> resolves to the
        // logging decorator that wraps it.
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(TestHandler) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);

        services.AddLogging();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IMessageHandler<TestMessage> handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TestMessage>>();
        handler.Should().BeOfType<LoggingMessageHandlerDecorator<TestMessage>>();
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

    [Fact]
    public void AddMessageContract_WhenRegistryRegisteredIncompatibly_Throws()
    {
        ServiceCollection services = new();
        services.AddSingleton<IMessageContractRegistry>(_ => new MessageContractRegistry());

        Action act = () => services.AddMessageContract<TestMessage>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddMessageContract_WhenRegistryPreRegisteredAsInstance_ReusesInstance()
    {
        ServiceCollection services = new();
        MessageContractRegistry preRegistered = new();
        services.AddSingleton<IMessageContractRegistry>(preRegistered);

        services.AddMessageContract<TestMessage>();

        using ServiceProvider provider = services.BuildServiceProvider();
        IMessageContractRegistry resolved = provider.GetRequiredService<IMessageContractRegistry>();

        resolved.Should().BeSameAs(preRegistered);
        resolved.TryResolve(typeof(TestMessage).FullName!, out _).Should().BeTrue();
    }

    #endregion

    #region Nested Types

    private sealed record TestMessage : Message;

    private sealed class TestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestDispatchMap : IMessageDispatchMap
    {
        public Task DispatchAsync(IServiceProvider serviceProvider, IMessage message, DispatchMode mode, CancellationToken cancellationToken)
        {
            return message switch
            {
                TestMessage typedMessage => MessageHandlerInvoker.InvokeAsync(serviceProvider, typedMessage, mode, cancellationToken),
                _ => throw new InvalidOperationException($"No dispatch entry for '{message.GetType().FullName}'."),
            };
        }

        public bool CanDispatch(Type messageType) => messageType == typeof(TestMessage);
    }

    #endregion
}
