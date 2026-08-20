using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Messaging;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

public sealed class LoggingMessageHandlerDecoratorTests
{
    #region Test Method Declarations

    [Fact]
    public async Task HandleAsync_InvokesInnerHandler()
    {
        Mock<IMessageHandler<TestMessage>> inner = new();

        LoggingMessageHandlerDecorator<TestMessage> decorator = new(
            inner.Object,
            NullLogger<LoggingMessageHandlerDecorator<TestMessage>>.Instance);

        TestMessage message = new();

        await decorator.HandleAsync(message, TestContext.Current.CancellationToken);

        inner.Verify(handler => handler.HandleAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenInnerThrows_PropagatesException()
    {
        Mock<IMessageHandler<TestMessage>> inner = new();
        inner
            .Setup(handler => handler.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        LoggingMessageHandlerDecorator<TestMessage> decorator = new(
            inner.Object,
            NullLogger<LoggingMessageHandlerDecorator<TestMessage>>.Instance);

        Func<Task> act = async () => await decorator.HandleAsync(new TestMessage(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_Throws()
    {
        LoggingMessageHandlerDecorator<TestMessage> decorator = new(
            Mock.Of<IMessageHandler<TestMessage>>(),
            NullLogger<LoggingMessageHandlerDecorator<TestMessage>>.Instance);

        Func<Task> act = async () => await decorator.HandleAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage : Message;

    #endregion
}
