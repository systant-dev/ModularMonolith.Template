using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Messaging;
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
        inboxStore
            .Setup(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(
            inner.Object,
            inboxStore.Object,
            "TestConsumer",
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
        inboxStore
            .Setup(store => store.TryMarkProcessedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        InboxIdempotencyMessageHandlerDecorator<TestMessage> decorator = new(
            inner.Object,
            inboxStore.Object,
            "TestConsumer",
            NullLogger<InboxIdempotencyMessageHandlerDecorator<TestMessage>>.Instance);

        await decorator.HandleAsync(new TestMessage(), TestContext.Current.CancellationToken);

        inner.Verify(handler => handler.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage : Message;

    #endregion
}
