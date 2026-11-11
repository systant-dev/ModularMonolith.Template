using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.HealthChecks;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.Api.Tests.HealthChecks;

public sealed class ModuleHealthCheckTests
{
    #region Test Method Declarations

    [Fact]
    public void EachModule_RegistersAReadinessCheckForItsDatabase()
    {
        ServiceCollection services = new();
        IHealthChecksBuilder healthChecks = services.AddHealthChecks();
        IModule[] modules = [new global::UserModule.Module(), new global::AccountsModule.Module()];

        foreach (IModule module in modules)
        {
            module.RegisterHealthChecks(healthChecks);
        }

        using ServiceProvider provider = services.BuildServiceProvider();
        ICollection<HealthCheckRegistration> registrations = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations;

        registrations.Select(registration => registration.Name).Should().BeEquivalentTo("UserModule-database", "AccountsModule-database");
        registrations.Should().OnlyContain(registration => registration.Tags.Contains(HealthCheckTags.Ready));
    }

    #endregion
}
