using ModularMonolith.Template.ModularGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

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
                using Microsoft.Extensions.Configuration;

                public static partial class ModuleServiceCollectionExtensions
                {
                    [ModularMonolith.Template.SharedKernel.Modular.ModuleRegistrationTarget]
                    private static partial void GetModules(IServiceCollection serviceCollection, IConfiguration configuration);
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