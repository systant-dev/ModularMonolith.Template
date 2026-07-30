using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace ModularMonolith.Template.MappingGen.Tests.Cacheable;

internal static class CacheableHelper
{
    #region Pubic Method Declarations

    public static (ImmutableArray<Diagnostic> Diagnostics, string[] Output) GetGeneratedTrees(string[] trackingNames, params string[] sources)
    {
        IEnumerable<PortableExecutableReference> references = ReferencesBuilder.Build();
        CSharpCompilation compilation = CompilationBuilder.Build(sources, references);
        GeneratorDriverRunResult runResult = RunGeneratorAndAssertOutput(compilation, trackingNames);
        return (runResult.Diagnostics, runResult.GeneratedTrees.Select(syntaxTree => syntaxTree.ToString()).ToArray());
    }

    #endregion

    #region Private Method Declarations

    /// <returns></returns>
    private static GeneratorDriverRunResult RunGeneratorAndAssertOutput(CSharpCompilation compilation, string[] trackingNames)
    {
        ISourceGenerator generator = new ApiGenerator.Generator().AsSourceGenerator();
        GeneratorDriverOptions driverOptions = new(disabledOutputs: IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);
        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: driverOptions);
        CSharpCompilation clone = compilation.Clone();

        driver = driver.RunGenerators(compilation);

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        GeneratorDriverRunResult runResult2 = driver.RunGenerators(clone).GetRunResult();

        AssertRunsEqual(runResult, runResult2, trackingNames);

        runResult2.Results[0]
                    .TrackedOutputSteps
                    .SelectMany(keyValuePair => keyValuePair.Value)
                    .SelectMany(runStep => runStep.Outputs)
                    .Should()
                    .OnlyContain(runStep => runStep.Reason == IncrementalStepRunReason.Cached);

        return runResult;
    }

    private static void AssertRunsEqual(GeneratorDriverRunResult runResult1, GeneratorDriverRunResult runResult2, string[] trackingNames)
    {
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        trackedSteps1.Should().NotBeEmpty().And.HaveSameCount(trackedSteps2).And.ContainKeys(trackedSteps2.Keys);

        foreach (KeyValuePair<string, ImmutableArray<IncrementalGeneratorRunStep>> keyValuePair in trackedSteps1)
        {
            string trackingName = keyValuePair.Key;
            ImmutableArray<IncrementalGeneratorRunStep> runSteps1 = keyValuePair.Value;
            ImmutableArray<IncrementalGeneratorRunStep> runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        return;
    }

    /// <returns></returns>
    private static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(GeneratorDriverRunResult runResult, string[] trackingNames)
    {
        return runResult.Results[0]
                        .TrackedSteps
                        .Where(step => trackingNames.Contains(step.Key))
                        .ToDictionary(trackingName => trackingName.Key, step => step.Value);
    }

    private static void AssertEqual(ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
                                    ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
                                    string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        int length = runSteps1.Length;
        for (int index = 0; index < length; index++)
        {
            IncrementalGeneratorRunStep runStep1 = runSteps1[index];
            IncrementalGeneratorRunStep runStep2 = runSteps2[index];
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(runStep => runStep.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(runStep => runStep.Value);

            outputs1.Should().Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            runStep2.Outputs.Should()
                            .OnlyContain
                             (
                                runStep => runStep.Reason == IncrementalStepRunReason.Cached || runStep.Reason == IncrementalStepRunReason.Unchanged,
                                $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {IncrementalStepRunReason.Unchanged}"
                             );

            AssertObjectGraph(runStep1, stepName);
        }
    }

    private static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
    {
        HashSet<object> visited = [];
        foreach ((object obj, IncrementalStepRunReason _) in runStep.Outputs)
        {
            Visit(obj, stepName, visited);
        }
    }

    private static void Visit(object? node, string stepName, HashSet<object> visited)
    {
        string validationReason = $"{stepName} shouldn't contain banned symbols";

        if (node is null || !visited.Add(node))
        {
            return;
        }

        node.Should().NotBeOfType<Compilation>(validationReason)
                     .And.NotBeOfType<ISymbol>(validationReason)
                     .And.NotBeOfType<SyntaxNode>(validationReason);

        Type type = node.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
        {
            return;
        }

        if (node is IEnumerable collection and not string)
        {
            foreach (object element in collection)
            {
                Visit(element, stepName, visited);
            }
            return;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            object? fieldValue = field.GetValue(node);
            Visit(fieldValue, stepName, visited);
        }
    }

    #endregion
}