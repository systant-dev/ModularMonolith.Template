using FluentAssertions;
using ModularMonolith.Template.Analysers.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;
using System.Collections.Immutable;
using System.Globalization;

namespace ModularMonolith.Template.Analysers.Tests.Analysers;

[Component(TestComponent.Analysers)]
[Category(TestCategory.Diagnostics)]
public sealed class ResultConsumedAnalyserTests
{
    #region Test Method Declarations

    [Fact]
    public async Task ReportsDiagnosticWhenResultIsDiscarded()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ResultConsumedAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.ResultMarkerTypes, TestSources.ResultDiscarded);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == ResultConsumedAnalyser.DiagnosticId).Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage(CultureInfo.InvariantCulture).Should().Contain("DoWork");
    }

    [Fact]
    public async Task ReportsDiagnosticWhenAwaitedResultIsDiscarded()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ResultConsumedAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.ResultMarkerTypes, TestSources.ResultDiscardedAsync);

        diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == ResultConsumedAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenResultIsAssigned()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ResultConsumedAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.ResultMarkerTypes, TestSources.ResultConsumedByAssignment);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ResultConsumedAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticForNonResultExpressionStatement()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ResultConsumedAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.ResultMarkerTypes, TestSources.NonResultExpressionStatement);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ResultConsumedAnalyser.DiagnosticId);
    }

    #endregion
}
