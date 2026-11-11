using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using System.Net;

namespace ModularMonolith.Template.Api.Tests.RequestPipeline;

public sealed class RateLimitingTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Constant Declarations

    private const int PermitLimit = 100;
    private const string UsersRoute = "/v1/users";

    #endregion

    #region Field Declarations

    private readonly TestWebApplicationFactory _factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task ExhaustingOneCallersLimit_DoesNotThrottleAnotherCaller()
    {
        using HttpClient noisyClient = CreateClient("noisy-caller");
        using HttpClient quietClient = CreateClient("quiet-caller");

        for (int request = 0; request < PermitLimit; request++)
        {
            HttpResponseMessage allowed = await noisyClient.GetAsync(UsersRoute, TestContext.Current.CancellationToken);
            allowed.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        HttpResponseMessage throttled = await noisyClient.GetAsync(UsersRoute, TestContext.Current.CancellationToken);
        HttpResponseMessage other = await quietClient.GetAsync(UsersRoute, TestContext.Current.CancellationToken);

        throttled.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        other.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Private Method Declarations

    private HttpClient CreateClient(string user)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeaderName, user);
        return client;
    }

    #endregion
}
