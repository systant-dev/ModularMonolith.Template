using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Configuration;

namespace ModularMonolith.Template.SharedKernel.Tests.Configuration;

public sealed class ModuleOptionsTests
{
    #region Test Method Declarations

    [Fact]
    public void Enabled_ShouldRetainAssignedValue_WhenSetToTrue()
    {
        TestModuleOptions options = new() { Enabled = true, Name = "Module" };

        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Enabled_ShouldRetainAssignedValue_WhenSetToFalse()
    {
        TestModuleOptions options = new() { Enabled = false, Name = "Module" };

        options.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ModuleOptions_ShouldImplementIModuleOptions()
    {
        TestModuleOptions options = new() { Enabled = true, Name = "Module" };

        options.Should().BeAssignableTo<IModuleOptions>();
    }

    [Fact]
    public void Records_WithSameValues_ShouldBeEqual()
    {
        TestModuleOptions first = new() { Enabled = true, Name = "Module" };
        TestModuleOptions second = new() { Enabled = true, Name = "Module" };

        first.Should().Be(second);
    }

    [Fact]
    public void Records_WithDifferentValues_ShouldNotBeEqual()
    {
        TestModuleOptions first = new() { Enabled = true, Name = "Module" };
        TestModuleOptions second = new() { Enabled = false, Name = "Module" };

        first.Should().NotBe(second);
    }

    #endregion
}