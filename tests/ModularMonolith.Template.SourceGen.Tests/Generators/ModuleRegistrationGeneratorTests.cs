using ModularMonolith.Template.SourceGen.Tests.Generators.Helpers;

namespace ModularMonolith.Template.SourceGen.Tests.Generators;

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