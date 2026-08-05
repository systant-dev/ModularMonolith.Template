using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.HealthChecks;

public sealed class HealthCheckEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    #region Field Declarations

    private readonly WebApplicationFactory<Program> factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task LivenessEndpoint_ReturnsHealthy()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessEndpoint_ReturnsSuccess()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
