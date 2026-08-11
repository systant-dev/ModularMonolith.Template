using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;
using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class MessageHandlerInvokerTests
{
    #region Test Method Declarations

    [Fact]
    public async Task InvokeAsync_InvokesAllRegisteredHandlers()
    {
        ConcurrentBag<string> log = [];
        ServiceProvider provider = BuildProvider(
            new RecordingHandler("first", log),
            new RecordingHandler("second", log));

        await MessageHandlerInvoker.InvokeAsync(provider, new TestMessage(), DispatchMode.Sequential, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo(["first", "second"]);
    }

    [Fact]
    public async Task InvokeAsync_WithNoHandlers_DoesNotThrow()
    {
        ServiceProvider provider = BuildProvider();

        Func<Task> act = async () => await MessageHandlerInvoker.InvokeAsync(
            provider, new TestMessage(), DispatchMode.Sequential, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_Concurrent_InvokesAllHandlers()
    {
        ConcurrentBag<string> log = [];
        ServiceProvider provider = BuildProvider(
            new RecordingHandler("a", log),
            new RecordingHandler("b", log),
            new RecordingHandler("c", log));

        await MessageHandlerInvoker.InvokeAsync(provider, new TestMessage(), DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenHandlerThrows_PropagatesException()
    {
        ServiceProvider provider = BuildProvider(new ThrowingHandler());

        Func<Task> act = async () => await MessageHandlerInvoker.InvokeAsync(
            provider, new TestMessage(), DispatchMode.Sequential, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    #endregion

    #region Private Method Declarations

    private static ServiceProvider BuildProvider(params IMessageHandler<TestMessage>[] handlers)
    {
        ServiceCollection services = new();
        foreach (IMessageHandler<TestMessage> handler in handlers)
        {
            services.AddSingleton(handler);
        }

        return services.BuildServiceProvider();
    }

    #endregion

    #region Nested Types

    private sealed record TestMessage : IMessage;

    private sealed class RecordingHandler(string name, ConcurrentBag<string> log) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(name);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("boom");
    }

    #endregion
}
