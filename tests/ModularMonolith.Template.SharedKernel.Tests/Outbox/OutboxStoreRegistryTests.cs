using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class OutboxStoreRegistryTests
{
    #region Test Method Declarations

    [Fact]
    public void Register_AddsStoreType()
    {
        OutboxStoreRegistry registry = new();

        registry.Register(typeof(FirstStore));

        registry.StoreTypes.Should().ContainSingle().Which.Should().Be<FirstStore>();
    }

    [Fact]
    public void Register_IsIdempotentForSameType()
    {
        OutboxStoreRegistry registry = new();

        registry.Register(typeof(FirstStore));
        registry.Register(typeof(FirstStore));

        registry.StoreTypes.Should().ContainSingle();
    }

    [Fact]
    public void Register_TracksMultipleDistinctTypes()
    {
        OutboxStoreRegistry registry = new();

        registry.Register(typeof(FirstStore));
        registry.Register(typeof(SecondStore));

        registry.StoreTypes.Should().BeEquivalentTo([typeof(FirstStore), typeof(SecondStore)]);
    }

    [Fact]
    public void Register_WithNullType_Throws()
    {
        OutboxStoreRegistry registry = new();

        Action act = () => registry.Register(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Nested Types

    private sealed class FirstStore;

    private sealed class SecondStore;

    #endregion
}
