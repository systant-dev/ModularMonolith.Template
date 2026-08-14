using FluentAssertions;
using ModularMonolith.Template.MappingGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.MappingGen.Tests.Cacheable;

[Component(TestComponent.MappingGen)]
[Category(TestCategory.Cacheability)]
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

    [Theory]
    [MemberData(nameof(Sources))]
    public void RemainsCachedAfterUnrelatedEdit(string source)
    {
        CacheableHelper.AssertUnchangedAfterUnrelatedEdit<MappingGenerator>(TrackingNames, source);
    }

    #endregion

    #region Public Method Declarations

    public static TheoryData<string> Sources()
    {
        return
        [
            TestSources.MatchingProperties,
            TestSources.CaseInsensitiveAndConvertible,
            TestSources.MultipleTargets,
            TestSources.NoInvocations,
        ];
    }

    #endregion
}
