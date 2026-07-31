using ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

public sealed class ModuleEndpointGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsMapEndpointsBodyFromAssemblyRegistrations()
    {
        string source = $$"""
            {{TestSources.MarkerTypes}}

            {{TestSources.ExampleModule}}

            namespace ModularMonolith.Template.Api.Modular
            {
                using Microsoft.AspNetCore.Routing;
                using Microsoft.Extensions.Configuration;

                public static partial class ModuleEndpointRouteBuilderExtensions
                {
                    [ModularMonolith.Template.SharedKernel.Modular.ModuleEndpointMappingTarget]
                    private static partial void MapEndpoints(IEndpointRouteBuilder endpoints, IConfiguration configuration);
                }
            }
            """;

        const string assemblyAttributes = """
            [assembly: ModularMonolith.Template.SharedKernel.Modular.ModuleRegistration<ExampleModuleA.Module>]
            """;

        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ModuleEndpointGenerator>(source, assemblyAttributes);
        return Verify(driver).UseFileName("ModuleEndpointGenerator");
    }

    #endregion
}
