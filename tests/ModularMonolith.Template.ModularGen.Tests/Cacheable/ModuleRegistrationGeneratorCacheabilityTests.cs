using FluentAssertions;
using ModularMonolith.Template.ModularGen.Generators;
using ModularMonolith.Template.ModularGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Cacheable;

public sealed class ModuleRegistrationGeneratorCacheabilityTests
{
    #region Field Declarations

    private static readonly string[] TrackingNames = ["ModuleRegistrations"];

    #endregion

    #region Test Method Declarations

    [Fact]
    public void ProducesCachedOutputOnSecondRun()
    {
        string source = $$"""
            {{TestSources.MarkerTypes}}

            {{TestSources.ExampleModule}}
            """;

        (System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, _) =
            CacheableHelper.GetGeneratedTrees<ModuleRegistrationGenerator>(TrackingNames, source);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void RemainsCachedAfterUnrelatedEdit()
    {
        string source = $$"""
            {{TestSources.MarkerTypes}}

            {{TestSources.ExampleModule}}
            """;

        CacheableHelper.AssertUnchangedAfterUnrelatedEdit<ModuleRegistrationGenerator>(TrackingNames, source);
    }

    #endregion
}