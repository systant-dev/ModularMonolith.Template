using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Handling;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

public sealed class InboxIdempotencyMessageHandlerDecoratorTests
{
    #region Test Method Declarations

    [Fact]
    public async Task HandleAsync_WhenNotYetProcessed_InvokesInnerHandler()
    {
        Mock<IMessageHandler<TestMessage>> inner = new();
        Mock<IInboxStore> inboxStore = new();
        inboxStore.Setup(store => store.IsProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);
        inboxStore.Setup(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(inner.Object, inboxStore.Object,"TestConsumer",
                                                                             NullLogger<InboxIdempotencyMessageHandlerDecorator<TestMessage>>.Instance);

        TestMessage message = new();

        await decorator.HandleAsync(message, TestContext.Current.CancellationToken);

        inboxStore.Verify(store => store.TryMarkProcessedAsync("TestConsumer", message.MessageId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        inner.Verify(handler => handler.HandleAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_SkipsInnerHandler()
    {
        Mock<IMessageHandler<TestMessage>> inner = new();
        Mock<IInboxStore> inboxStore = new();
        inboxStore.Setup(store => store.IsProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(inner.Object, inboxStore.Object,
                                                                             "TestConsumer",
                                                                              NullLogger<InboxIdempotencyMessageHandlerDecorator<TestMessage>>.Instance);

        await decorator.HandleAsync(new TestMessage(), TestContext.Current.CancellationToken);

        inner.Verify(handler => handler.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        inboxStore.Verify(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerThrows_DoesNotMarkProcessed()
    {
        Mock<IMessageHandler<TestMessage>> inner = new();
        inner.Setup(handler => handler.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("handler failed"));
        Mock<IInboxStore> inboxStore = new();

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(inner.Object, inboxStore.Object,
                                                                             "TestConsumer",
                                                                             NullLogger<InboxIdempotencyMessageHandlerDecorator<TestMessage>>.Instance);

        Func<Task> act = () => decorator.HandleAsync(new TestMessage(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
        inboxStore.Verify(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MarksProcessedOnlyAfterInnerHandlerCompletes()
    {
        List<string> calls = [];
        Mock<IMessageHandler<TestMessage>> inner = new();
        inner.Setup(handler => handler.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
             .Callback(() => calls.Add("handle"))
             .Returns(Task.CompletedTask);

        Mock<IInboxStore> inboxStore = new();
        inboxStore.Setup(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Callback(() => calls.Add("mark"))
                  .ReturnsAsync(true);

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(inner.Object, inboxStore.Object,
                                                                             "TestConsumer",
                                                                             NullLogger<InboxIdempotencyMessageHandlerDecorator<TestMessage>>.Instance);

        await decorator.HandleAsync(new TestMessage(), TestContext.Current.CancellationToken);

        calls.Should().Equal("handle", "mark");
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage : Message;

    #endregion
}
