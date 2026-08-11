using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Outbox;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Configuration)]
public sealed class OutboxHostExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public async Task MigrateModuleDatabasesAsync_InvokesAllRegisteredInitialisers()
    {
        Mock<IModuleDatabaseInitialiser> first = new();
        Mock<IModuleDatabaseInitialiser> second = new();

        ServiceCollection services = new();
        services.AddScoped(_ => first.Object);
        services.AddScoped(_ => second.Object);
        using ServiceProvider provider = services.BuildServiceProvider();

        await provider.MigrateModuleDatabasesAsync(TestContext.Current.CancellationToken);

        first.Verify(initialiser => initialiser.InitialiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        second.Verify(initialiser => initialiser.InitialiseAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MigrateModuleDatabasesAsync_WithNoInitialisers_DoesNotThrow()
    {
        ServiceCollection services = new();
        using ServiceProvider provider = services.BuildServiceProvider();

        Func<Task> act = async () => await provider.MigrateModuleDatabasesAsync(TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    #endregion
}
