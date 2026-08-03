using FluentAssertions;
using ModularMonolith.Template.Analysers.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers.Tests.Analysers;

[Component(TestComponent.Analysers)]
[Category(TestCategory.Diagnostics)]
public sealed class ModuleAttributeAnalyserTests
{
    #region Test Method Declarations

    [Fact]
    public async Task ReportsDiagnosticWhenModuleAttributeAppliedWithoutIModule()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleAttributeAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.MarkerTypes, TestSources.ModuleMissingInterface);
        Diagnostic diagnostic = diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == ModuleAttributeAnalyser.DiagnosticId).Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("BadModule");
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenModuleImplementsIModule()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleAttributeAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.MarkerTypes, TestSources.ModuleImplementingInterface);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleAttributeAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenTypeHasNoModuleAttribute()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleAttributeAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.MarkerTypes, TestSources.TypeWithoutModuleAttribute);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleAttributeAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenIModuleIsInheritedFromBaseClass()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleAttributeAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.MarkerTypes, TestSources.ModuleInheritingInterfaceFromBase);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleAttributeAnalyser.DiagnosticId);
    }

    #endregion
}