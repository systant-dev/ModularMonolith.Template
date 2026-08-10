using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Configuration)]
public sealed class OutboxOptionsTests
{
    #region Test Method Declarations

    [Fact]
    public void Defaults_AreProductionSafe()
    {
        OutboxOptions options = new();

        options.PollingInterval.Should().Be(TimeSpan.FromSeconds(10));
        options.BatchSize.Should().Be(20);
        options.MaxAttempts.Should().Be(5);
    }

    [Fact]
    public void SectionName_IsOutbox()
    {
        OutboxOptions.SectionName.Should().Be("Outbox");
    }

    #endregion
}
