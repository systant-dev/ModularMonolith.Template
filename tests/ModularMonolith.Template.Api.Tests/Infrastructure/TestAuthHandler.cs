using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Security;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

/// <summary>
/// Authentication handler used by integration tests to satisfy the API's
/// secure-by-default authorization policy without a real identity provider.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    #region Field Declarations

    public const string SchemeName = "Test";

    #endregion

    #region Protected Method Declarations

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, "test-user"),
            new(ClaimTypes.Name, "Test User"),
            new(AuthorizationScopes.ScopeClaimType, AuthorizationScopes.Read),
            new(AuthorizationScopes.ScopeClaimType, AuthorizationScopes.Write),
        ];

        ClaimsIdentity identity = new(claims, SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    #endregion
}
