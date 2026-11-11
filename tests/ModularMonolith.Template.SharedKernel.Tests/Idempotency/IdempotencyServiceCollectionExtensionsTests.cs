using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Idempotency;

namespace ModularMonolith.Template.SharedKernel.Tests.Idempotency;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class IdempotencyServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddIdempotency_RegistersInMemoryStore()
    {
        ServiceCollection services = new();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        IIdempotencyStore store = provider.GetRequiredService<IIdempotencyStore>();

        store.Should().BeOfType<InMemoryIdempotencyStore>();
    }

    [Fact]
    public void AddIdempotency_ReturnsSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddIdempotency();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddIdempotency_RegistersStoreAsSingleton()
    {
        ServiceCollection services = new();

        services.AddIdempotency();

        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IIdempotencyStore) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    #endregion
}
