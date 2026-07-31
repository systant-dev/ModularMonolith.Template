using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.SharedKernel.Tests.Modular;

public sealed class ModuleEndpointMappingTargetAttributeTests
{
    #region Test Method Declarations

    [Fact]
    public void ModuleEndpointMappingTargetAttribute_ShouldHaveExpectedAttributeUsage()
    {
        AttributeUsageAttribute usage = typeof(ModuleEndpointMappingTargetAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().Be(AttributeTargets.Method);
        usage.AllowMultiple.Should().BeFalse();
    }

    #endregion
}
