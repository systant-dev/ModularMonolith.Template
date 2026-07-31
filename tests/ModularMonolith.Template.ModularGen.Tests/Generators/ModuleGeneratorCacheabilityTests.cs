using FluentAssertions;
using ModularMonolith.Template.ModularGen.Tests.Cacheable;
using ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

public sealed class ModuleGeneratorCacheabilityTests
{
    #region Field Declarations

    private static readonly string[] TrackingNames = ["GetModules"];

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

        (System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, _) =
            CacheableHelper.GetGeneratedTrees<ModuleGenerator>(TrackingNames, source, assemblyAttributes);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    #endregion
}