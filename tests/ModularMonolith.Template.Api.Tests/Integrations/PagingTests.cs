using AccountsModule.Contracts.Responses;
using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using ModularMonolith.Template.SharedKernel.Idempotency;
using System.Net.Http.Json;
using UserModule.Contracts.Responses;

namespace ModularMonolith.Template.Api.Tests.Integrations;

public sealed class PagingTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Constant Declarations

    private const int DefaultPageSize = 20;
    private const string TotalCountHeaderName = "X-Total-Count";

    #endregion

    #region Field Declarations

    private readonly TestWebApplicationFactory _factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task GetAll_WithoutLimit_ReturnsAtMostTheDefaultPageSize()
    {
        using HttpClient client = _factory.CreateClient();
        await CreateUsersAsync(client, DefaultPageSize + 1);

        IReadOnlyList<UserResponse>? users = await client.GetFromJsonAsync<IReadOnlyList<UserResponse>>("/v1/users", TestContext.Current.CancellationToken);
        IReadOnlyList<AccountResponse>? accounts = await client.GetFromJsonAsync<IReadOnlyList<AccountResponse>>("/v1/accounts", TestContext.Current.CancellationToken);

        users.Should().HaveCount(DefaultPageSize);
        accounts.Should().NotBeNull();
        accounts!.Count.Should().BeLessThanOrEqualTo(DefaultPageSize);
    }

    [Fact]
    public async Task GetAllAccounts_WithLimitAndCount_ReturnsOnePageAndTheTotal()
    {
        using HttpClient client = _factory.CreateClient();
        await CreateAccountAsync(client, userId: 9001);
        await CreateAccountAsync(client, userId: 9002);

        HttpResponseMessage response = await client.GetAsync("/v1/accounts?limit=1&includeCount=true", TestContext.Current.CancellationToken);
        IReadOnlyList<AccountResponse>? page = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountResponse>>(TestContext.Current.CancellationToken);

        page.Should().ContainSingle();
        int total = int.Parse(response.Headers.GetValues(TotalCountHeaderName).Single(), System.Globalization.CultureInfo.InvariantCulture);
        total.Should().BeGreaterThanOrEqualTo(2);
    }

    [Theory]
    [InlineData("/v1/users", "{\"fullName\":\"Location User\",\"email\":\"location.user@example.com\"}")]
    [InlineData("/v1/accounts", "{\"userId\":9101,\"accountHolderName\":\"Location Holder\",\"initialDeposit\":0}")]
    public async Task Create_ReturnsALocationThatResolvesToTheNewResource(string route, string json)
    {
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Post, route) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        request.Headers.Add(IdempotencyFilter.IdempotencyKeyHeaderName, Guid.NewGuid().ToString("n"));

        HttpResponseMessage created = await client.SendAsync(request, TestContext.Current.CancellationToken);

        created.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        created.Headers.Location.Should().NotBeNull();
        created.Headers.Location!.AbsolutePath.Should().StartWith($"{route}/");

        HttpResponseMessage fetched = await client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);
        fetched.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    #endregion

    #region Private Method Declarations

    private static async Task CreateUsersAsync(HttpClient client, int count)
    {
        for (int index = 0; index < count; index++)
        {
            var request = new { fullName = $"Paging User {index}", email = $"paging.user.{index}@example.com" };
            HttpResponseMessage response = await PostAsync(client, "/v1/users", request);
            response.EnsureSuccessStatusCode();
        }
    }

    private static async Task CreateAccountAsync(HttpClient client, int userId)
    {
        var request = new { userId, accountHolderName = $"Paging Holder {userId}", initialDeposit = 0 };
        HttpResponseMessage response = await PostAsync(client, "/v1/accounts", request);
        response.EnsureSuccessStatusCode();
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string route, object body)
    {
        HttpRequestMessage request = new(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
        request.Headers.Add(IdempotencyFilter.IdempotencyKeyHeaderName, Guid.NewGuid().ToString("n"));
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    #endregion
}
