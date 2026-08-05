namespace ModularMonolith.Template.SharedKernel.Security;

/// <summary>
/// Claim types and scope values used to authorize access to module endpoints.
/// Shared so modules and the host agree on the same claim/scope vocabulary.
/// </summary>
public static class AuthorizationScopes
{
    #region Field Declarations

    /// <summary>The claim type Microsoft Entra ID uses for delegated scopes.</summary>
    public const string ScopeClaimType = "scp";

    /// <summary>Scope granting read access to the API.</summary>
    public const string Read = "api.read";

    /// <summary>Scope granting write access to the API.</summary>
    public const string Write = "api.write";

    #endregion
}
