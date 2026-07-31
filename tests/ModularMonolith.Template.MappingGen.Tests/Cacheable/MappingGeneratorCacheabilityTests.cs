using FluentAssertions;
using ModularMonolith.Template.MappingGen.Tests.Generators.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.MappingGen.Tests.Cacheable;

public sealed class MappingGeneratorCacheabilityTests
{
    #region Field Declarations

    private static readonly string[] TrackingNames = ["MapMethods", "MapTargets"];

    #endregion

    #region Test Method Declarations

    [Theory]
    [MemberData(nameof(Sources))]
    public void ProducesCachedOutputOnSecondRun(string source)
    {
        (System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, _) = CacheableHelper.GetGeneratedTrees<MappingGenerator>(TrackingNames, source);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    public static TheoryData<string> Sources() =>
    [
        TestSources.MatchingProperties,
        TestSources.CaseInsensitiveAndConvertible,
        TestSources.MultipleTargets,
        TestSources.NoInvocations,
    ];

    [Theory]
    [MemberData(nameof(Sources))]
    public void RemainsCachedAfterUnrelatedEdit(string source)
    {
        CacheableHelper.AssertUnchangedAfterUnrelatedEdit<MappingGenerator>(TrackingNames, source);
    }

    #endregion
}