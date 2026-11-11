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
    private const string AccountsModuleNamespace = "AccountsModule";

    #endregion

    #region Field Declarations

    private static readonly Assembly SharedKernelAssembly = typeof(IModule).Assembly;

    #endregion

    #region Property Declarations

    public static TheoryData<string> ModuleNamespaces => [UserModuleNamespace, AccountsModuleNamespace];

    #endregion

    #region Test Method Declarations

    [Theory]
    [MemberData(nameof(ModuleNamespaces))]
    public void Modules_ShouldNotDependOn_TheHostApi(string moduleNamespace)
    {
        TestResult result = Types.InAssembly(ModuleAssembly(moduleNamespace))
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "modules must not depend on the host API. Offending types: {0}",
            FailingTypes(result));
    }

    [Theory]
    [MemberData(nameof(ModuleNamespaces))]
    public void SharedKernel_ShouldNotDependOn_AnyModule(string moduleNamespace)
    {
        TestResult result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn($"{moduleNamespace}.")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the shared kernel must not depend on module implementations. Offending types: {0}",
            FailingTypes(result));
    }

    [Theory]
    [MemberData(nameof(ModuleNamespaces))]
    public void ModuleImplementations_ShouldBeInternal(string moduleNamespace)
    {
        TestResult result = Types.InAssembly(ModuleAssembly(moduleNamespace))
            .That()
            .ResideInNamespace(moduleNamespace)
            .And()
            .DoNotResideInNamespace($"{moduleNamespace}.Migrations")
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

    [Theory]
    [InlineData(UserModuleNamespace, AccountsModuleNamespace)]
    [InlineData(AccountsModuleNamespace, UserModuleNamespace)]
    public void Modules_ShouldOnlyDependOn_AnotherModulesContracts(string moduleNamespace, string otherModuleNamespace)
    {
        TestResult result = Types.InAssembly(ModuleAssembly(moduleNamespace))
            .ShouldNot()
            .HaveDependencyOnAny($"{otherModuleNamespace}.Features", $"{otherModuleNamespace}.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "a module may only use another module through its Contracts. Offending types: {0}",
            FailingTypes(result));
    }

    #endregion

    #region Private Method Declarations

    private static Assembly ModuleAssembly(string moduleNamespace)
    {
        return moduleNamespace == UserModuleNamespace
            ? typeof(global::UserModule.Module).Assembly
            : typeof(global::AccountsModule.Module).Assembly;
    }

    private static string FailingTypes(TestResult result)
    {
        return result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty;
    }

    #endregion
}
