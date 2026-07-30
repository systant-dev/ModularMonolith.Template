using ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

public sealed class ModuleGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsGetModulesBodyFromAssemblyRegistrations()
    {
        string source = $$"""
            {{TestSources.MarkerTypes}}

            {{TestSources.ExampleModule}}

            namespace ModularMonolith.Template.Api.Modular
            {
                using Microsoft.Extensions.DependencyInjection;

                public static partial class ModuleServiceCollectionExtensions
                {
                    [ModularMonolith.Template.SharedKernel.Modular.ModuleRegistrationTarget]
                    private static partial void GetModules(IServiceCollection serviceCollection);
                }
            }
            """;

        const string assemblyAttributes = """
            [assembly: ModularMonolith.Template.SharedKernel.Modular.ModuleRegistration<ExampleModuleA.Module>]
            """;

        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ModuleGenerator>(source, assemblyAttributes);
        return Verify(driver).UseFileName("ModuleGenerator");
    }

    #endregion
}