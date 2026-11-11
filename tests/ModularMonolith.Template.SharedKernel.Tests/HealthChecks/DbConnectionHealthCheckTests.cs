using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularMonolith.Template.SharedKernel.HealthChecks;
using ModularMonolith.Template.SharedKernel.Storage;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.HealthChecks;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Storage)]
public sealed class DbConnectionHealthCheckTests
{
    #region Constant Declarations

    private const string ConnectionFactoryKey = "TestModule";

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsUnreachable_ReportsUnhealthy()
    {
        InvalidOperationException failure = new("connection refused");
        Mock<IDbConnectionFactory> connectionFactory = new();
        connectionFactory.Setup(factory => factory.CreateOpenConnectionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(failure);

        ServiceCollection services = new();
        services.AddLogging();
        services.AddKeyedSingleton(ConnectionFactoryKey, connectionFactory.Object);
        services.AddHealthChecks().AddModuleDatabaseCheck(ConnectionFactoryKey);
        await using ServiceProvider provider = services.BuildServiceProvider();

        HealthReport report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync(TestContext.Current.CancellationToken);

        report.Status.Should().Be(HealthStatus.Unhealthy);
        HealthReportEntry entry = report.Entries.Should().ContainKey($"{ConnectionFactoryKey}-database").WhoseValue;
        entry.Exception.Should().BeSameAs(failure);
        entry.Tags.Should().Contain(HealthCheckTags.Ready);
    }

    #endregion
}
