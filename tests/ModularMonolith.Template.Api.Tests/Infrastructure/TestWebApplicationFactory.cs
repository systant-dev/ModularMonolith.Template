using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.Api.Tests.Infrastructure;

/// <summary>
/// Test host that replaces the production Microsoft Entra ID authentication with
/// a always-authenticated test scheme so integration tests can exercise
/// endpoints protected by the secure-by-default authorization policy.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    #region Protected Method Declarations

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    #endregion
}
