using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.SharedKernel.Tests.Modular;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Modular)]
public sealed class ModuleAttributeTests
{
    #region Test Method Declarations

    [Fact]
    public void ModuleAttribute_ShouldHaveExpectedAttributeUsage()
    {
        AttributeUsageAttribute usage = typeof(ModuleAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().Be(AttributeTargets.Class);
        usage.AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void ModuleAttribute_CanDecorateClass()
    {
        typeof(DecoratedClass).GetCustomAttributes(typeof(ModuleAttribute), inherit: false)
                              .Should().ContainSingle();
    }

    #endregion

    #region Nested Types

    [Module]
    private sealed class DecoratedClass;

    #endregion
}