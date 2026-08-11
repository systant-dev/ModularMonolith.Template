using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ModularMonolith.Template.Api.Tests.Integrations;

public sealed class ProblemDetailsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task InvalidCreateRequest_ReturnsValidationProblemDetails()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/v1/users",
            new { FullName = "", Email = "not-an-email" },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task ProblemDetails_IncludesCustomInstanceAndRequestId()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/v1/users",
            new { FullName = "", Email = "" },
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        JsonElement root = document.RootElement;

        root.GetProperty("instance").GetString().Should().Be("POST /v1/users");
        root.TryGetProperty("requestId", out _).Should().BeTrue();
    }

    #endregion
}
