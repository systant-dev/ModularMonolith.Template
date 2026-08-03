using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.SharedKernel.Tests.Modular;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Modular)]
public sealed class ModuleRegistrationTargetAttributeTests
{
    #region Test Method Declarations

    [Fact]
    public void ModuleRegistrationTargetAttribute_ShouldHaveExpectedAttributeUsage()
    {
        AttributeUsageAttribute usage = typeof(ModuleRegistrationTargetAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().Be(AttributeTargets.Method);
        usage.AllowMultiple.Should().BeFalse();
    }

    #endregion
}