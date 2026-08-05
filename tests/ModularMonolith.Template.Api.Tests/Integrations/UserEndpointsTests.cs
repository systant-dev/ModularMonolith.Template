using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.Integrations;

public sealed class UserEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    #region Field Declarations

    private readonly WebApplicationFactory<Program> factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task GetAllUsers_ReturnsSuccess()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/users", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Response_IncludesCorrelationIdHeader()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/users", TestContext.Current.CancellationToken);

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
    }

    #endregion
}