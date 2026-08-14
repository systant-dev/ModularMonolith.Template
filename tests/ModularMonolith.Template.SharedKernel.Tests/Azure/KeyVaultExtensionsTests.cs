using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ModularMonolith.Template.SharedKernel.Azure;

namespace ModularMonolith.Template.SharedKernel.Tests.Azure;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Azure)]
public sealed class KeyVaultExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void ConnectKeyVault_ShouldThrow_WhenConfiguredUrlIsMissing()
    {
        ConfigurationManager configurationManager = new();

        Action act = () => configurationManager.ConnectKeyVault(isDevelopment: true);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*KeyVault:Url*");
    }

    [Fact]
    public void ConnectKeyVault_ShouldThrow_WhenCustomKeyIsMissing()
    {
        ConfigurationManager configurationManager = new();

        Action act = () => configurationManager.ConnectKeyVault(isDevelopment: true, key: "Custom:Key");

        act.Should().Throw<InvalidOperationException>().WithMessage("*Custom:Key*");
    }

    #endregion
}
