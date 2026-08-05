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
                .AddPolicy(AuthorizationPolicies.RequireUser, policy => policy
                    .RequireAuthenticatedUser())
                .AddPolicy(AuthorizationPolicies.ReadAccess, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthorizationScopes.ScopeClaimType, AuthorizationScopes.Read, AuthorizationScopes.Write))
                .AddPolicy(AuthorizationPolicies.WriteAccess, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthorizationScopes.ScopeClaimType, AuthorizationScopes.Write));

            return serviceCollection;
        }
    }
}
