using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Features;

namespace ModularMonolith.Template.SharedKernel.Tests.Features;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class FeatureFlagFilterTests
{
    #region Test Method Declarations

    [Fact]
    public async Task InvokeAsync_WhenFeatureEnabled_InvokesNext()
    {
        object sentinel = new();
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(sentinel);
        };

        FeatureFlagFilter filter = new("SampleFeature");
        EndpointFilterInvocationContext context = CreateContext(featureEnabled: true);

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeTrue();
        result.Should().BeSameAs(sentinel);
    }

    [Fact]
    public async Task InvokeAsync_WhenFeatureDisabled_ReturnsNotFoundAndDoesNotInvokeNext()
    {
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(null);
        }

        FeatureFlagFilter filter = new("SampleFeature");
        EndpointFilterInvocationContext context = CreateContext(featureEnabled: false);

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeFalse();
        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task InvokeAsync_WhenFeatureAbsent_ReturnsNotFound()
    {
        static ValueTask<object?> next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(null);

        FeatureFlagFilter filter = new("MissingFeature");
        EndpointFilterInvocationContext context = CreateContext(featureEnabled: true);

        object? result = await filter.InvokeAsync(context, next);

        result.Should().BeOfType<NotFound>();
    }

    #endregion

    #region Private Method Declarations

    private static EndpointFilterInvocationContext CreateContext(bool featureEnabled)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{FeatureFlagFilter.ConfigurationSectionName}:SampleFeature"] = featureEnabled ? "true" : "false"
            })
            .Build();

        ServiceProvider provider = new ServiceCollection()
            .AddSingleton(configuration)
            .BuildServiceProvider();

        DefaultHttpContext httpContext = new() { RequestServices = provider };
        return EndpointFilterInvocationContext.Create(httpContext);
    }

    #endregion
}
