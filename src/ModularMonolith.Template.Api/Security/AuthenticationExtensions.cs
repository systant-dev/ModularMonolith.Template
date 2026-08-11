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
            serviceCollection
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration.GetSection(ConfigurationSectionName));

            return serviceCollection;
        }

        public IServiceCollection AddModuleAuthorization()
        {
            serviceCollection
                .AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build())
                .AddPolicy(AuthorisationPolicies.RequireUser, policy => policy
                    .RequireAuthenticatedUser())
                .AddPolicy(AuthorisationPolicies.ReadAccess, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthorisationScopes.ScopeClaimType, AuthorisationScopes.Read, AuthorisationScopes.Write))
                .AddPolicy(AuthorisationPolicies.WriteAccess, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthorisationScopes.ScopeClaimType, AuthorisationScopes.Write));

            return serviceCollection;
        }
    }
}
