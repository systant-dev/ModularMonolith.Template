using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Http;
using System.Net;

namespace ModularMonolith.Template.SharedKernel.Tests.Http;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class HttpClientBuilderExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void ConfigureSharedPrimaryHandler_ShouldReturnSameBuilder()
    {
        ServiceCollection services = new();
        IHttpClientBuilder builder = services.AddHttpClient("test");

        IHttpClientBuilder result = builder.ConfigureSharedPrimaryHandler();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void ConfigureSharedPrimaryHandler_ShouldConfigurePrimaryHandlerAsSocketsHttpHandler()
    {
        ServiceCollection services = new();
        IHttpClientBuilder builder = services.AddHttpClient("test");
        builder.ConfigureSharedPrimaryHandler();

        using ServiceProvider provider = services.BuildServiceProvider();
        HttpMessageHandlerFactory factory = new(provider);
        SocketsHttpHandler handler = factory.GetPrimaryHandler("test");

        handler.AutomaticDecompression.Should().Be(DecompressionMethods.All);
        handler.PooledConnectionLifetime.Should().Be(TimeSpan.FromMinutes(5));
        handler.EnableMultipleHttp2Connections.Should().BeTrue();
    }

    [Fact]
    public void AddSharedResilienceHandler_ShouldReturnSameBuilder()
    {
        ServiceCollection services = new();
        IHttpClientBuilder builder = services.AddHttpClient("test");

        IHttpClientBuilder result = builder.AddSharedResilienceHandler();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddSharedResilienceHandler_ShouldApplyDefaultTotalTimeout()
    {
        ServiceCollection services = new();
        IHttpClientBuilder builder = services.AddHttpClient("test");
        builder.AddSharedResilienceHandler();

        using ServiceProvider provider = services.BuildServiceProvider();
        HttpStandardResilienceOptions options = GetResilienceOptions(provider, "test");

        options.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromMinutes(5));
        options.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(100));
        options.Retry.MaxRetryAttempts.Should().Be(3);
    }

    [Fact]
    public void AddSharedResilienceHandler_ShouldApplyCustomValues()
    {
        ServiceCollection services = new();
        IHttpClientBuilder builder = services.AddHttpClient("custom");
        builder.AddSharedResilienceHandler(TimeSpan.FromSeconds(30), maxRetryAttempts: 5);

        using ServiceProvider provider = services.BuildServiceProvider();
        HttpStandardResilienceOptions options = GetResilienceOptions(provider, "custom");

        options.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(10));
        options.Retry.MaxRetryAttempts.Should().Be(5);
    }

    #endregion

    #region Private Method Declarations

    private static HttpStandardResilienceOptions GetResilienceOptions(IServiceProvider provider, string clientName)
    {
        IOptionsMonitor<HttpStandardResilienceOptions> monitor =
            provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>();

        return monitor.Get($"{clientName}-standard");
    }

    #endregion

    #region Nested Types

    private sealed class HttpMessageHandlerFactory(IServiceProvider provider)
    {
        #region Public Method Declarations

        public SocketsHttpHandler GetPrimaryHandler(string name)
        {
            IHttpMessageHandlerFactory factory = provider.GetRequiredService<IHttpMessageHandlerFactory>();
            HttpMessageHandler handler = factory.CreateHandler(name);

            HttpMessageHandler current = handler;
            while (current is DelegatingHandler delegating && delegating.InnerHandler is not null)
            {
                current = delegating.InnerHandler;
            }

            return (SocketsHttpHandler)current;
        }

        #endregion
    }

    #endregion
}