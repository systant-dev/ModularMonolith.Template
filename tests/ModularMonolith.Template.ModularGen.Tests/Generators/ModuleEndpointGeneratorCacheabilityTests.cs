using FluentAssertions;
using ModularMonolith.Template.ModularGen.Tests.Cacheable;
using ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

public sealed class ModuleEndpointGeneratorCacheabilityTests
{
    #region Field Declarations

    private static readonly string[] TrackingNames = ["GetEndpointModules"];

    #endregion

    #region Test Method Declarations

    [Fact]
    public void ProducesCachedOutputOnSecondRun()
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

        (System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, _) =
            CacheableHelper.GetGeneratedTrees<ModuleEndpointGenerator>(TrackingNames, source, assemblyAttributes);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void RemainsCachedAfterUnrelatedEdit()
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

        CacheableHelper.AssertUnchangedAfterUnrelatedEdit<ModuleEndpointGenerator>(TrackingNames, source, assemblyAttributes);
    }

    #endregion
}
