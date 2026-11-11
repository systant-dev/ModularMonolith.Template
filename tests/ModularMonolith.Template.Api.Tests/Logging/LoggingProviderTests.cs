using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using Serilog.Extensions.Logging;

namespace ModularMonolith.Template.Api.Tests.Logging;

public sealed class LoggingProviderTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory _factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public void ConsoleOutput_ComesFromSerilogOnly()
    {
        IEnumerable<ILoggerProvider> providers = _factory.Services.GetServices<ILoggerProvider>();

        providers.Should().ContainSingle(provider => provider is SerilogLoggerProvider);
        providers.Should().NotContain(provider => provider is ConsoleLoggerProvider);
    }

    #endregion
}
