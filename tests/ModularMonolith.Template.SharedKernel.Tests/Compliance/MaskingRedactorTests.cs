using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Compliance;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class MaskingRedactorTests
{
    #region Test Method Declarations

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("Jane Doe")]
    public void GetRedactedLength_ReturnsInputLength(string input)
    {
        MaskingRedactor redactor = new();

        int length = redactor.GetRedactedLength(input.AsSpan());

        length.Should().Be(input.Length);
    }

    [Fact]
    public void Redact_ReplacesEveryCharacterWithMask()
    {
        MaskingRedactor redactor = new();
        const string source = "Jane Doe";
        Span<char> destination = new char[source.Length];

        int written = redactor.Redact(source.AsSpan(), destination);

        written.Should().Be(source.Length);
        destination.ToString().Should().Be(new string('*', source.Length));
    }

    [Fact]
    public void Redact_WithEmptySource_WritesNothing()
    {
        MaskingRedactor redactor = new();
        Span<char> destination = Span<char>.Empty;

        int written = redactor.Redact(ReadOnlySpan<char>.Empty, destination);

        written.Should().Be(0);
    }

    [Fact]
    public void Redact_DoesNotLeakOriginalCharacters()
    {
        MaskingRedactor redactor = new();
        const string source = "secret@example.com";
        Span<char> destination = new char[source.Length];

        redactor.Redact(source.AsSpan(), destination);

        destination.ToString().Should().NotContain("secret").And.NotContain("@");
    }

    #endregion
}
