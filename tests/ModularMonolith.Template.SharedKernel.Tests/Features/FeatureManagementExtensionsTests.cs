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

    [Fact]
    public void MapModuleFeatureFlags_ProjectsModuleFlagToPrefixedGlobalFeature()
    {
        ConfigurationManager configurationManager = new();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:UserModule:FeatureManagement:UserDeletion"] = "false"
        });

        configurationManager.MapModuleFeatureFlags();

        configurationManager["FeatureManagement:UserModule.UserDeletion"].Should().Be("false");
    }

    [Fact]
    public void MapModuleFeatureFlags_ProjectsFlagsForEveryModule()
    {
        ConfigurationManager configurationManager = new();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:UserModule:FeatureManagement:UserDeletion"] = "true",
            ["Modules:AccountsModule:FeatureManagement:AccountsDeletion"] = "false"
        });

        configurationManager.MapModuleFeatureFlags();

        configurationManager["FeatureManagement:UserModule.UserDeletion"].Should().Be("true");
        configurationManager["FeatureManagement:AccountsModule.AccountsDeletion"].Should().Be("false");
    }

    [Fact]
    public void MapModuleFeatureFlags_PreservesComplexFeatureFilterSubtree()
    {
        ConfigurationManager configurationManager = new();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:UserModule:FeatureManagement:UserDeletion:EnabledFor:0:Name"] = "Percentage",
            ["Modules:UserModule:FeatureManagement:UserDeletion:EnabledFor:0:Parameters:Value"] = "50"
        });

        configurationManager.MapModuleFeatureFlags();

        configurationManager["FeatureManagement:UserModule.UserDeletion:EnabledFor:0:Name"].Should().Be("Percentage");
        configurationManager["FeatureManagement:UserModule.UserDeletion:EnabledFor:0:Parameters:Value"].Should().Be("50");
    }

    [Fact]
    public void MapModuleFeatureFlags_ReturnsSameConfigurationManager()
    {
        ConfigurationManager configurationManager = new();

        ConfigurationManager result = configurationManager.MapModuleFeatureFlags();

        result.Should().BeSameAs(configurationManager);
    }

    [Fact]
    public void MapModuleFeatureFlags_WhenNoModuleFeatureFlags_AddsNoFeatureManagementSection()
    {
        ConfigurationManager configurationManager = new();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:UserModule:Enabled"] = "true"
        });

        configurationManager.MapModuleFeatureFlags();

        configurationManager.GetSection("FeatureManagement").GetChildren().Should().BeEmpty();
    }

    #endregion
}
