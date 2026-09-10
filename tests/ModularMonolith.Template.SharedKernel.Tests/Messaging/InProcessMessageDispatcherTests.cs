using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Dispatch;
using ModularMonolith.Template.SharedKernel.Messaging.Handling;
using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class InProcessMessageDispatcherTests
{
    #region Test Method Declarations

    [Fact]
    public async Task PublishSequentiallyAsync_InvokesHandlersInRegistrationOrder()
    {
        List<string> log = [];
        RecordingHandler firstHandler = new("first", log);
        RecordingHandler secondHandler = new("second", log);
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(firstHandler);
            services.AddSingleton<IMessageHandler<TestMessage>>(secondHandler);
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        TestMessage message = new();
        await dispatcher.PublishAsync(message, DispatchMode.Sequential, TestContext.Current.CancellationToken);

        log.Should().Equal("first", "second");
    }

    [Fact]
    public async Task PublishSequentiallyAsync_WithNoHandlers_Completes()
    {
        ServiceProvider provider = BuildProvider(_ => { });
        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        TestMessage message = new();
        Func<Task> act = () => dispatcher.PublishAsync(message, DispatchMode.Sequential, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishSequentiallyAsync_WithNullMessage_Throws()
    {
        ServiceProvider provider = BuildProvider(_ => { });
        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        Func<Task> act = () => dispatcher.PublishAsync(null!, DispatchMode.Sequential, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishConcurrentlyAsync_InvokesAllHandlers()
    {
        ConcurrentBag<string> log = [];
        ConcurrentRecordingHandler handlerA = new("a", log);
        ConcurrentRecordingHandler handlerB = new("b", log);
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(handlerA);
            services.AddSingleton<IMessageHandler<TestMessage>>(handlerB);
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        TestMessage message = new();
        await dispatcher.PublishAsync(message, DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo("a", "b");
    }

    [Fact]
    public async Task PublishConcurrentlyAsync_RunsEveryHandlerEvenWhenOneFails()
    {
        ConcurrentBag<string> log = [];
        ConcurrentRecordingHandler okHandler = new("ok", log);
        ThrowingHandler throwingHandler = new("boom", log);
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(okHandler);
            services.AddSingleton<IMessageHandler<TestMessage>>(throwingHandler);
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        TestMessage message = new();
        Func<Task> act = () => dispatcher.PublishAsync(message, DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
        log.Should().BeEquivalentTo("ok", "boom");
    }

    [Fact]
    public async Task PublishConcurrentlyAsync_WithNullMessage_Throws()
    {
        ServiceProvider provider = BuildProvider(_ => { });
        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        Func<Task> act = () => dispatcher.PublishAsync(null!, DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Private Method Declarations

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        ServiceCollection services = new();
        services.AddInProcessMessaging();
        services.AddSingleton<IMessageDispatchMap, TestDispatchMap>();
        configure(services);
        return services.BuildServiceProvider();
    }

    #endregion

    #region Nested Types

    private sealed record TestMessage : Message;

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

        public bool CanDispatch(Type messageType)
        {
            return messageType == typeof(TestMessage);
        }
    }

    private sealed class RecordingHandler(string name, List<string> log) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(name);
            return Task.CompletedTask;
        }
    }

    private sealed class ConcurrentRecordingHandler(string name, ConcurrentBag<string> log) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(name);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler(string name, ConcurrentBag<string> log) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(name);
            throw new InvalidOperationException(name);
        }
    }

    #endregion
}
