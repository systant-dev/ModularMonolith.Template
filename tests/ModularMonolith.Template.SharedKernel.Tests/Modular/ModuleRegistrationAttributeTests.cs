using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.SharedKernel.Tests.Modular;

public sealed class ModuleRegistrationAttributeTests
{
    private sealed class TestModule : IModule
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
        }

        public void MapEndpoints()
        {
        }
    }

    #region Test Method Declarations

    [Fact]
    public void ModuleType_ShouldReturnGenericArgumentType()
    {
        ModuleRegistrationAttribute<TestModule> attribute = new();

        attribute.ModuleType.Should().Be<TestModule>();
    }

    [Fact]
    public void ModuleRegistrationAttribute_ShouldHaveExpectedAttributeUsage()
    {
        AttributeUsageAttribute usage = typeof(ModuleRegistrationAttribute<>)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().Be(AttributeTargets.Assembly);
        usage.AllowMultiple.Should().BeTrue();
    }

    #endregion
}