using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Idempotency;

namespace ModularMonolith.Template.SharedKernel.Tests.Idempotency;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class InMemoryIdempotencyStoreTests
{
    #region Test Method Declarations

    [Fact]
    public async Task TryReserveAsync_WhenKeyIsNew_ReturnsTrue()
    {
        IIdempotencyStore store = CreateStore();
        bool reserved = await store.TryReserveAsync("key-1", TestContext.Current.CancellationToken);
        reserved.Should().BeTrue();
    }

    [Fact]
    public async Task TryReserveAsync_WhenKeyReserved_ReturnsFalseOnSecondCall()
    {
        IIdempotencyStore store = CreateStore();

        bool first = await store.TryReserveAsync("key-1", TestContext.Current.CancellationToken);
        bool second = await store.TryReserveAsync("key-1", TestContext.Current.CancellationToken);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveAsync_WhenDifferentKeys_ReturnsTrueForEach()
    {
        IIdempotencyStore store = CreateStore();

        bool first = await store.TryReserveAsync("key-1", TestContext.Current.CancellationToken);
        bool second = await store.TryReserveAsync("key-2", TestContext.Current.CancellationToken);

        first.Should().BeTrue();
        second.Should().BeTrue();
    }

    #endregion

    #region Private Method Declarations

    private static IIdempotencyStore CreateStore()
    {
        ServiceProvider provider = new ServiceCollection().AddIdempotency().BuildServiceProvider();
        return provider.GetRequiredService<IIdempotencyStore>();
    }

    #endregion
}
