using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Modular;
using NetArchTest.Rules;
using System.Reflection;
using TestResult = NetArchTest.Rules.TestResult;

namespace ModularMonolith.Template.Api.Tests.Architecture;

public sealed class ModuleIsolationTests
{
    #region Constant Declarations

    private const string ApiNamespace = "ModularMonolith.Template.Api";
    private const string UserModuleNamespace = "UserModule";
    private const string UserModuleImplementationNamespace = "UserModule.";

    #endregion

    #region Field Declarations

    private static readonly Assembly UserModuleAssembly = typeof(global::UserModule.Module).Assembly;
    private static readonly Assembly SharedKernelAssembly = typeof(IModule).Assembly;

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Modules_ShouldNotDependOn_TheHostApi()
    {
        TestResult result = Types.InAssembly(UserModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "modules must not depend on the host API. Offending types: {0}",
            FailingTypes(result));
    }

    [Fact]
    public void SharedKernel_ShouldNotDependOn_AnyModule()
    {
        TestResult result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn(UserModuleImplementationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the shared kernel must not depend on module implementations. Offending types: {0}",
            FailingTypes(result));
    }

    [Fact]
    public void ModuleImplementations_ShouldBeInternal()
    {
        TestResult result = Types.InAssembly(UserModuleAssembly)
            .That()
            .ResideInNamespace(UserModuleNamespace)
            .And()
            .DoNotResideInNamespace("UserModule.Migrations")
            .And()
            .DoNotHaveName(nameof(global::UserModule.Module))
            .And()
            .AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "module internals must not be public; only contracts are shared. Offending types: {0}",
            FailingTypes(result));
    }

    #endregion

    #region Private Method Declarations

    private static string FailingTypes(TestResult result)
    {
        return result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty;
    }

    #endregion
}
