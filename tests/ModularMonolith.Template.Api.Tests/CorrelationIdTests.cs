using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModularMonolith.Template.Api.Tests;

public sealed class CorrelationIdTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    #region Field Declarations

    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    private readonly WebApplicationFactory<Program> factory = factory;

    #endregion

    #region Test Methods

    [Fact]
    public async Task Response_EchoesClientProvidedCorrelationId()
    {
        HttpClient client = factory.CreateClient();
        const string correlationId = "test-correlation-id-123";

        using HttpRequestMessage request = new(HttpMethod.Get, "/v1/users");
        request.Headers.Add(CorrelationIdHeaderName, correlationId);

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.Headers.GetValues(CorrelationIdHeaderName).Should().ContainSingle().Which.Should().Be(correlationId);
    }

    [Fact]
    public async Task Response_GeneratesCorrelationIdWhenNoneProvided()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/users", TestContext.Current.CancellationToken);

        response.Headers.GetValues(CorrelationIdHeaderName).Should().ContainSingle().Which.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
