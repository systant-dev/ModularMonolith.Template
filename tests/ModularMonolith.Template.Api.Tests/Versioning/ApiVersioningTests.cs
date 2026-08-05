using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.Versioning;

public sealed class ApiVersioningTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task VersionedRoute_ReturnsSuccess()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/users", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnversionedRoute_ReturnsNotFound()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/users", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
