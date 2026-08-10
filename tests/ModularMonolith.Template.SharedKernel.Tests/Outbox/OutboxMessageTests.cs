using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class OutboxMessageTests
{
    #region Test Method Declarations

    [Fact]
    public void Create_WithMessage_PopulatesMetadata()
    {
        TestMessage message = new("hello");

        OutboxMessage outboxMessage = OutboxMessage.Create(message);

        outboxMessage.Id.Should().NotBeEmpty();
        outboxMessage.OccurredOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        outboxMessage.Type.Should().Be(typeof(TestMessage).AssemblyQualifiedName);
        outboxMessage.Payload.Should().Contain("hello");
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        outboxMessage.Attempts.Should().Be(0);
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullMessage_Throws()
    {
        Action act = () => OutboxMessage.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialise_RoundTripsOriginalMessage()
    {
        TestMessage original = new("round-trip");
        OutboxMessage outboxMessage = OutboxMessage.Create(original);

        IMessage? deserialised = outboxMessage.Deserialise();

        deserialised.Should().BeOfType<TestMessage>()
            .Which.Value.Should().Be("round-trip");
    }

    [Fact]
    public void Deserialise_WithUnknownType_ReturnsNull()
    {
        OutboxMessage outboxMessage = new()
        {
            Id = Guid.NewGuid(),
            Type = "Some.Unknown.Type, Nonexistent.Assembly",
            Payload = "{}"
        };

        outboxMessage.Deserialise().Should().BeNull();
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage(string Value) : IMessage;

    #endregion
}
