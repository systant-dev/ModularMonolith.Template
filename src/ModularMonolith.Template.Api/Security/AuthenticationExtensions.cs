using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using ModularMonolith.Template.SharedKernel.Security;

namespace ModularMonolith.Template.Api.Security;

public static class AuthenticationExtensions
{
    #region Field Declarations

    public const string ConfigurationSectionName = "AzureAd";

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleAuthentication(IConfiguration configuration)
        {
            serviceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                             .AddMicrosoftIdentityWebApi(configuration.GetSection(ConfigurationSectionName));

            return serviceCollection;
        }

        public IServiceCollection AddModuleAuthorization()
        {
            AuthorizationPolicy fallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                                                 .Build();

            serviceCollection.AddAuthorizationBuilder()
                             .SetFallbackPolicy(fallbackPolicy)
                             .AddPolicy(AuthorisationPolicies.RequireUser, policy => policy.RequireAuthenticatedUser())
                             // RequireScope (Microsoft.Identity.Web) rather than RequireClaim: Entra sends every
                             // granted scope in one space-separated claim, and JWT inbound claim mapping can
                             // rename "scp", so an exact match on the claim value would reject real tokens.
                             .AddPolicy(AuthorisationPolicies.ReadAccess, policy => policy
                                 .RequireAuthenticatedUser()
                                 .RequireScope(AuthorisationScopes.Read, AuthorisationScopes.Write))
                             .AddPolicy(AuthorisationPolicies.WriteAccess, policy => policy
                                 .RequireAuthenticatedUser()
                                 .RequireScope(AuthorisationScopes.Write));

            return serviceCollection;
        }
    }
}
