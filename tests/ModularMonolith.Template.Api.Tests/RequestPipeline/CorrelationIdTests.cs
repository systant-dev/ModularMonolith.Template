using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;

namespace ModularMonolith.Template.Api.Tests.RequestPipeline;

public sealed class CorrelationIdTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    private readonly TestWebApplicationFactory factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task Response_EchoesClientProvidedCorrelationId()
    {
        using HttpClient client = factory.CreateClient();
        const string CorrelationId = "test-correlation-id-123";

        using HttpRequestMessage request = new(HttpMethod.Get, "/v1/users");
        request.Headers.Add(CorrelationIdHeaderName, CorrelationId);

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.Headers.GetValues(CorrelationIdHeaderName).Should().ContainSingle().Which.Should().Be(CorrelationId);
    }

    [Fact]
    public async Task Response_GeneratesCorrelationIdWhenNoneProvided()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/users", TestContext.Current.CancellationToken);

        response.Headers.GetValues(CorrelationIdHeaderName).Should().ContainSingle().Which.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Response_GeneratesCorrelationIdWhenClientProvidesEmptyValue()
    {
        using HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, "/v1/users");
        request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, string.Empty);

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.Headers.GetValues(CorrelationIdHeaderName).Should().ContainSingle().Which.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
