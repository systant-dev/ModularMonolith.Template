using FluentAssertions;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using ModularMonolith.Template.SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;

namespace ModularMonolith.Template.Api.Tests.Security;

public sealed class ScopeAuthorisationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Constant Declarations

    private const string MappedScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
    private const string UsersRoute = "/v1/users";
    private const string MissingUserRoute = "/v1/users/999999";

    #endregion

    #region Field Declarations

    private readonly TestWebApplicationFactory _factory = factory;

    #endregion

    #region Test Method Declarations

    [Theory]
    [InlineData(AuthorisationScopes.ScopeClaimType)]
    [InlineData(MappedScopeClaimType)]
    public async Task SpaceSeparatedScopes_GrantReadAndWrite(string claimType)
    {
        using HttpClient client = CreateClient($"{AuthorisationScopes.Read} {AuthorisationScopes.Write}", claimType);

        HttpResponseMessage read = await client.GetAsync(UsersRoute, TestContext.Current.CancellationToken);
        HttpResponseMessage write = await UpdateMissingUserAsync(client);

        read.StatusCode.Should().Be(HttpStatusCode.OK);
        write.StatusCode.Should().Be(HttpStatusCode.NotFound, "the caller is authorised, the user simply does not exist");
    }

    [Fact]
    public async Task ReadScopeOnly_CanReadButNotWrite()
    {
        using HttpClient client = CreateClient(AuthorisationScopes.Read, AuthorisationScopes.ScopeClaimType);

        HttpResponseMessage read = await client.GetAsync(UsersRoute, TestContext.Current.CancellationToken);
        HttpResponseMessage write = await UpdateMissingUserAsync(client);

        read.StatusCode.Should().Be(HttpStatusCode.OK);
        write.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WriteScopeOnly_CanAlsoRead()
    {
        using HttpClient client = CreateClient(AuthorisationScopes.Write, AuthorisationScopes.ScopeClaimType);

        HttpResponseMessage read = await client.GetAsync(UsersRoute, TestContext.Current.CancellationToken);

        read.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnrelatedScope_IsForbidden()
    {
        using HttpClient client = CreateClient("other.scope", AuthorisationScopes.ScopeClaimType);

        HttpResponseMessage read = await client.GetAsync(UsersRoute, TestContext.Current.CancellationToken);

        read.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Private Method Declarations

    private HttpClient CreateClient(string scopes, string claimType)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopesHeaderName, scopes);
        client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeClaimTypeHeaderName, claimType);
        return client;
    }

    private static Task<HttpResponseMessage> UpdateMissingUserAsync(HttpClient client)
    {
        var request = new { fullName = "Ada Lovelace", email = "ada.scope@example.com" };
        return client.PutAsJsonAsync(MissingUserRoute, request, TestContext.Current.CancellationToken);
    }

    #endregion
}
