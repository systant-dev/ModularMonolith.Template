namespace ModularMonolith.Template.SharedKernel.Security;

/// <summary>
/// Authorization policy names shared between the host (which registers the
/// policies) and modules (which apply them to their endpoints), avoiding
/// duplicated magic strings across the module boundary.
/// </summary>
public static class AuthorizationPolicies
{
    #region Field Declarations

    /// <summary>Requires an authenticated user with no additional claims.</summary>
    public const string RequireUser = "RequireUser";

    /// <summary>Requires a token containing a read scope.</summary>
    public const string ReadAccess = "ReadAccess";

    /// <summary>Requires a token containing a write scope.</summary>
    public const string WriteAccess = "WriteAccess";

    #endregion
}
