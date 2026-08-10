using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class MessageContractAttributeTests
{
    #region Test Method Declarations

    [Fact]
    public void Constructor_WithName_StoresName()
    {
        MessageContractAttribute attribute = new("user.created.v1");

        attribute.Name.Should().Be("user.created.v1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespace_Throws(string? name)
    {
        Action act = () => _ = new MessageContractAttribute(name!);

        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
