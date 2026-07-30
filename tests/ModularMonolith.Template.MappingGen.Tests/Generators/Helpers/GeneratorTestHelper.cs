using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Tests.Generators.Helpers;

internal static class GeneratorTestHelper
{
    #region Internal Method Declarations

    public static CSharpCompilation CreateCompilation(params string[] sources)
    {
        IEnumerable<SyntaxTree> syntaxTrees = sources.Select(static source => CSharpSyntaxTree.ParseText(source));

        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "ModularMonolith.Template.Tests",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static GeneratorDriver RunGenerator<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        CSharpCompilation compilation = CreateCompilation(source);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);

        ImmutableArray<Diagnostic> outputDiagnostics = [.. outputCompilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        if (outputDiagnostics.Length > 0)
        {
            throw new InvalidOperationException(
                "The generated compilation contained errors:" + Environment.NewLine +
                string.Join(Environment.NewLine, outputDiagnostics.Select(static diagnostic => diagnostic.ToString())));
        }

        return driver;
    }

    public static ImmutableArray<Diagnostic> RunGeneratorDiagnostics<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        CSharpCompilation compilation = CreateCompilation(source);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Diagnostics;
    }

    #endregion
}
