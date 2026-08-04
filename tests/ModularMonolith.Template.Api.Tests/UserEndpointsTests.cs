using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModularMonolith.Template.Api.Tests;

public sealed class UserEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    #region Field Declarations

    private readonly WebApplicationFactory<Program> factory = factory;

    #endregion

    #region Test Methods

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
