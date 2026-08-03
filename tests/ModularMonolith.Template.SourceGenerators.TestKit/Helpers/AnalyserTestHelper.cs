using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

public static class AnalyserTestHelper
{
    #region Public Method Declarations

    public static Task<ImmutableArray<Diagnostic>> RunAnalyserAsync<TAnalyser>(CancellationToken cancellationToken, params string[] sources)
        where TAnalyser : DiagnosticAnalyzer, new()
    {
        CSharpCompilation compilation = GeneratorTestHelper.CreateCompilation(sources);
        TAnalyser analyser = new();
        ImmutableArray<DiagnosticAnalyzer> analysers = [analyser];
        CompilationWithAnalyzers compilationWithAnalysers = compilation.WithAnalyzers(analysers);
        return compilationWithAnalysers.GetAnalyzerDiagnosticsAsync(cancellationToken);
    }

    #endregion
}