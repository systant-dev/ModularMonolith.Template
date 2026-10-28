using ModularMonolith.Template.ModularGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

[Component(TestComponent.ModularGen)]
[Category(TestCategory.Generators)]
public sealed class ModuleRegistrationGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsAssemblyRegistrationForModuleType()
    {
        string source = $$"""
            {{TestSources.MarkerTypes}}

            {{TestSources.ExampleModule}}
            """;

        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ModuleRegistrationGenerator>(source);
        return Verify(driver).UseFileName("ModuleRegistrationGenerator");
    }

    #endregion
}
