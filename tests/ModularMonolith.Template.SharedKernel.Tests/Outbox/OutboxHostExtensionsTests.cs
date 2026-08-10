using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Configuration)]
public sealed class OutboxHostExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public async Task MigrateModuleDatabasesAsync_InvokesAllRegisteredInitialisers()
    {
        RecordingInitialiser first = new();
        RecordingInitialiser second = new();

        ServiceCollection services = new();
        services.AddScoped<IModuleDatabaseInitialiser>(_ => first);
        services.AddScoped<IModuleDatabaseInitialiser>(_ => second);
        using ServiceProvider provider = services.BuildServiceProvider();

        await provider.MigrateModuleDatabasesAsync(TestContext.Current.CancellationToken);

        first.Invoked.Should().BeTrue();
        second.Invoked.Should().BeTrue();
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

    #region Nested Types

    private sealed class RecordingInitialiser : IModuleDatabaseInitialiser
    {
        public bool Invoked { get; private set; }

        public Task InitialiseAsync(CancellationToken cancellationToken = default)
        {
            Invoked = true;
            return Task.CompletedTask;
        }
    }

    #endregion
}
