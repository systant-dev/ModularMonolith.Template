using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Tests.Generators.Helpers;

internal static class GeneratorTestHelper
{
    #region Internal Method Declarations

    public static GeneratorDriver RunGenerator<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        List<SyntaxTree> syntaxTrees = [CSharpSyntaxTree.ParseText(source)];

        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "ModularMonolith.Template.Tests",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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

    #endregion
}
