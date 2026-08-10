using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;
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
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(new RecordingHandler("first", log));
            services.AddSingleton<IMessageHandler<TestMessage>>(new RecordingHandler("second", log));
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        await dispatcher.PublishAsync(new TestMessage(), DispatchMode.Sequential, TestContext.Current.CancellationToken);

        log.Should().Equal("first", "second");
    }

    [Fact]
    public async Task PublishSequentiallyAsync_WithNoHandlers_Completes()
    {
        ServiceProvider provider = BuildProvider(_ => { });
        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        Func<Task> act = () => dispatcher.PublishAsync(new TestMessage(), DispatchMode.Sequential, TestContext.Current.CancellationToken);

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
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(new ConcurrentRecordingHandler("a", log));
            services.AddSingleton<IMessageHandler<TestMessage>>(new ConcurrentRecordingHandler("b", log));
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        await dispatcher.PublishAsync(new TestMessage(), DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo("a", "b");
    }

    [Fact]
    public async Task PublishConcurrentlyAsync_RunsEveryHandlerEvenWhenOneFails()
    {
        ConcurrentBag<string> log = [];
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IMessageHandler<TestMessage>>(new ConcurrentRecordingHandler("ok", log));
            services.AddSingleton<IMessageHandler<TestMessage>>(new ThrowingHandler("boom", log));
        });

        IMessageDispatcher dispatcher = provider.GetRequiredService<IMessageDispatcher>();

        Func<Task> act = () => dispatcher.PublishAsync(new TestMessage(), DispatchMode.Concurrent, TestContext.Current.CancellationToken);

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
        configure(services);
        return services.BuildServiceProvider();
    }

    #endregion

    #region Nested Types

    private sealed record TestMessage : IMessage;

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
