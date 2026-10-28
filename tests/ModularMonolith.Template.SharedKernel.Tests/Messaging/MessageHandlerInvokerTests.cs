using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Dispatch;
using ModularMonolith.Template.SharedKernel.Messaging.Handling;
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
        RecordingHandler firstHandler = new("first", log);
        RecordingHandler secondHandler = new("second", log);
        ServiceProvider provider = BuildProvider(firstHandler, secondHandler);

        TestMessage message = new();
        await MessageHandlerInvoker.InvokeAsync(provider, message, DispatchMode.Sequential, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo(["first", "second"]);
    }

    [Fact]
    public async Task InvokeAsync_WithNoHandlers_DoesNotThrow()
    {
        ServiceProvider provider = BuildProvider();

        TestMessage message = new();
        Func<Task> act = async () => await MessageHandlerInvoker.InvokeAsync(
            provider, message, DispatchMode.Sequential, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_Concurrent_InvokesAllHandlers()
    {
        ConcurrentBag<string> log = [];
        RecordingHandler handlerA = new("a", log);
        RecordingHandler handlerB = new("b", log);
        RecordingHandler handlerC = new("c", log);
        ServiceProvider provider = BuildProvider(
            handlerA,
            handlerB,
            handlerC);

        TestMessage message = new();
        await MessageHandlerInvoker.InvokeAsync(provider, message, DispatchMode.Concurrent, TestContext.Current.CancellationToken);

        log.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenHandlerThrows_PropagatesException()
    {
        ThrowingHandler throwingHandler = new();
        ServiceProvider provider = BuildProvider(throwingHandler);

        TestMessage message = new();
        Func<Task> act = async () => await MessageHandlerInvoker.InvokeAsync(
            provider, message, DispatchMode.Sequential, TestContext.Current.CancellationToken);

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

    private sealed record TestMessage : Message;

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
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("boom");
        }
    }

    #endregion
}
