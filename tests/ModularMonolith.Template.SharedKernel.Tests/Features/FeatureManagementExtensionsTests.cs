using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using ModularMonolith.Template.SharedKernel.Features;

namespace ModularMonolith.Template.SharedKernel.Tests.Features;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class FeatureManagementExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddModuleFeatureManagement_ReturnsBuilder()
    {
        ServiceCollection services = new();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        IFeatureManagementBuilder builder = services.AddModuleFeatureManagement();

        builder.Should().NotBeNull();
    }

    [Fact]
    public void AddModuleFeatureManagement_RegistersFeatureManager()
    {
        ServiceCollection services = new();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddModuleFeatureManagement();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetService<IFeatureManager>().Should().NotBeNull();
    }

    [Fact]
    public void ConnectAppConfigurationFeatureFlags_WhenOptionalAndUnconfigured_ReturnsFalse()
    {
        ConfigurationManager configurationManager = new();

        bool connected = configurationManager.ConnectAppConfigurationFeatureFlags(isDevelopment: true);

        connected.Should().BeFalse();
    }

    [Fact]
    public void ConnectAppConfigurationFeatureFlags_WhenRequiredAndUnconfigured_Throws()
    {
        ConfigurationManager configurationManager = new();

        Action act = () => configurationManager.ConnectAppConfigurationFeatureFlags(isDevelopment: true, optional: false);

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion
}
