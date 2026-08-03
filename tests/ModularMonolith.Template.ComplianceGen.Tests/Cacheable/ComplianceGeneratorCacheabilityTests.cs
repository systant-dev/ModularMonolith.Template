using FluentAssertions;
using ModularMonolith.Template.ComplianceGen.Generators;
using ModularMonolith.Template.ComplianceGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ComplianceGen.Tests.Cacheable;

[Component(TestComponent.ComplianceGen)]
[Category(TestCategory.Cacheability)]
public sealed class ComplianceGeneratorCacheabilityTests
{
    #region Field Declarations

    private static readonly string[] TrackingNames = ["ClassificationProfiles"];

    #endregion

    #region Test Method Declarations

    [Theory]
    [MemberData(nameof(Sources))]
    public void ProducesCachedOutputOnSecondRun(string source)
    {
        (System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, _) =
            CacheableHelper.GetGeneratedTrees<ComplianceGenerator>(TrackingNames, source, TestSources.Abstractions);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [MemberData(nameof(Sources))]
    public void RemainsCachedAfterUnrelatedEdit(string source)
    {
        CacheableHelper.AssertUnchangedAfterUnrelatedEdit<ComplianceGenerator>(TrackingNames, source, TestSources.Abstractions);
    }

    #endregion

    #region Public Method Declarations

    public static TheoryData<string> Sources() =>
    [
        TestSources.SimpleProfile,
        TestSources.NestedProfile,
        TestSources.NoProfiles,
    ];

    #endregion
}