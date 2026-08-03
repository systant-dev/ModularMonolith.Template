using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.SharedKernel.Tests.Modular;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Modular)]
public sealed class ModuleRegistrationAttributeTests
{
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

    #region Nested Types

    private sealed class TestModule : IModule
    {
        #region Public Method Declarations

        public void RegisterServices(IServiceCollection serviceCollection)
        {
        }

        public void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
        }

        #endregion
    }

    #endregion
}