using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Compliance.Redaction;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class RedactorTests
{
    #region Test Method Declarations

    [Fact]
    public void PassthroughRedactor_ShouldReturnValueUnchanged()
    {
        PassthroughRedactor.Instance.Redact("hello").Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PassthroughRedactor_ShouldReturnEmpty_WhenNullOrEmpty(string? value)
    {
        PassthroughRedactor.Instance.Redact(value).Should().BeEmpty();
    }

    [Fact]
    public void ErasingRedactor_ShouldMaskNonEmptyValue()
    {
        new ErasingRedactor().Redact("secret").Should().Be("***");
    }

    [Fact]
    public void ErasingRedactor_ShouldUseCustomMask()
    {
        new ErasingRedactor("[redacted]").Redact("secret").Should().Be("[redacted]");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ErasingRedactor_ShouldReturnEmpty_WhenNullOrEmpty(string? value)
    {
        new ErasingRedactor().Redact(value).Should().BeEmpty();
    }

    [Fact]
    public void HmacRedactor_ShouldBeDeterministicForSameKeyAndValue()
    {
        using HmacRedactor first = new("key-material");
        using HmacRedactor second = new("key-material");

        first.Redact("user@example.com").Should().Be(second.Redact("user@example.com"));
    }

    [Fact]
    public void HmacRedactor_ShouldDifferForDifferentKeys()
    {
        using HmacRedactor first = new("key-a");
        using HmacRedactor second = new("key-b");

        first.Redact("user@example.com").Should().NotBe(second.Redact("user@example.com"));
    }

    [Fact]
    public void HmacRedactor_ShouldNotReturnOriginalValue()
    {
        using HmacRedactor redactor = new("key-material");

        redactor.Redact("user@example.com").Should().NotBe("user@example.com");
    }

    [Fact]
    public void HmacRedactor_ShouldThrow_WhenKeyIsEmpty()
    {
        Action act = () => _ = new HmacRedactor([]);

        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
