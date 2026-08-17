using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Tests.Messaging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class MessageContractRegistryTests
{
    #region Test Method Declarations

    [Fact]
    public void GetContractName_WithoutAttribute_ReturnsFullName()
    {
        MessageContractRegistry registry = new();

        registry.GetContractName<PlainMessage>().Should().Be(typeof(PlainMessage).FullName);
    }

    [Fact]
    public void GetContractName_WithAttribute_ReturnsAttributeName()
    {
        MessageContractRegistry registry = new();

        registry.GetContractName<NamedMessage>().Should().Be("test.named.v1");
    }

    [Fact]
    public void TryResolve_WhenRegisteredByAttributeName_ReturnsType()
    {
        MessageContractRegistry registry = new();
        registry.Register<NamedMessage>();

        registry.TryResolve("test.named.v1", out Type resolved).Should().BeTrue();
        resolved.Should().Be<NamedMessage>();
    }

    [Fact]
    public void TryResolve_WhenNotRegisteredByFullName_ReturnsFalse()
    {
        MessageContractRegistry registry = new();

        registry.TryResolve(typeof(PlainMessage).FullName!, out Type resolved).Should().BeFalse();
        resolved.Should().BeNull();
    }

    [Fact]
    public void TryResolve_WithAssemblyQualifiedName_ResolvesViaTypeGetType()
    {
        MessageContractRegistry registry = new();

        registry.TryResolve(typeof(PlainMessage).AssemblyQualifiedName!, out Type resolved).Should().BeTrue();
        resolved.Should().Be<PlainMessage>();
    }

    [Fact]
    public void TryResolve_WithUnknownName_ReturnsFalse()
    {
        MessageContractRegistry registry = new();

        registry.TryResolve("nope.not.a.message", out Type resolved).Should().BeFalse();
        resolved.Should().BeNull();
    }

    #endregion

    #region Nested Types

    public sealed record PlainMessage : Message;

    [MessageContract("test.named.v1")]
    public sealed record NamedMessage : Message;

    #endregion
}
