using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.Integrations;

public sealed class UserEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory factory = factory;

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