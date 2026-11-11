using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.HealthChecks;

public sealed class HealthCheckEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory _factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task LivenessEndpoint_ReturnsHealthy()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessEndpoint_ReturnsSuccess()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
