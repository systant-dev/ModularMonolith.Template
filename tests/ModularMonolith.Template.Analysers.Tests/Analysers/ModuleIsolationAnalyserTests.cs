using FluentAssertions;
using ModularMonolith.Template.Analysers.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers.Tests.Analysers;

[Component(TestComponent.Analysers)]
[Category(TestCategory.Diagnostics)]
public sealed class ModuleIsolationAnalyserTests
{
    #region Test Method Declarations

    [Fact]
    public async Task ReportsDiagnosticWhenReferencingAnotherModuleInternal()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleIsolationAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.CrossModuleTypes, TestSources.ModuleReferencingOtherModuleInternal);

        diagnostics.Should().Contain(diagnostic => diagnostic.Id == ModuleIsolationAnalyser.DiagnosticId);
        diagnostics.Where(diagnostic => diagnostic.Id == ModuleIsolationAnalyser.DiagnosticId)
                   .Should().OnlyContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning)
                   .And.Contain(diagnostic => diagnostic.GetMessage().Contains("UserStore"));
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenReferencingAnotherModuleContracts()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleIsolationAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.CrossModuleTypes, TestSources.ModuleReferencingOtherModuleContracts);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleIsolationAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenReferencingOwnModuleInternal()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleIsolationAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.CrossModuleTypes, TestSources.ModuleReferencingOwnModuleInternal);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleIsolationAnalyser.DiagnosticId);
    }

    [Fact]
    public async Task DoesNotReportDiagnosticWhenConsumerIsNotAModule()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyserTestHelper.RunAnalyserAsync<ModuleIsolationAnalyser>
                                                 (TestContext.Current.CancellationToken, TestSources.CrossModuleTypes, TestSources.NonModuleReferencingModuleInternal);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == ModuleIsolationAnalyser.DiagnosticId);
    }

    #endregion
}
