using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using ModularMonolith.Template.SharedKernel.Security;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

public sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    #region Field Declarations

    public const string SchemeName = "Test";

    public const string UserHeaderName = "X-Test-User";

    public const string ScopesHeaderName = "X-Test-Scopes";

    public const string ScopeClaimTypeHeaderName = "X-Test-Scope-Claim-Type";

    #endregion

    #region Protected Method Declarations

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, Request.Headers.TryGetValue(UserHeaderName, out StringValues user) ? user.ToString() : "test-user"),
            new(ClaimTypes.Name, "Test User"),
            .. CreateScopeClaims(),
        ];

        ClaimsIdentity identity = new(claims, SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    #endregion

    #region Private Method Declarations

    private Claim[] CreateScopeClaims()
    {
        if (!Request.Headers.TryGetValue(ScopesHeaderName, out StringValues scopes))
        {
            return
            [
                new(AuthorisationScopes.ScopeClaimType, AuthorisationScopes.Read),
                new(AuthorisationScopes.ScopeClaimType, AuthorisationScopes.Write),
            ];
        }

        string claimType = Request.Headers.TryGetValue(ScopeClaimTypeHeaderName, out StringValues requestedClaimType)
            ? requestedClaimType.ToString()
            : AuthorisationScopes.ScopeClaimType;

        return [new(claimType, scopes.ToString())];
    }

    #endregion
}
