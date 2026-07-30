using FluentAssertions;
using ModularMonolith.Template.MappingGen.Tests.Generators.Helpers;

namespace ModularMonolith.Template.MappingGen.Tests.Generators;

public sealed class MappingGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsMapAttributeDuringPostInitialization()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.NoInvocations);

        string attribute = GetGeneratedSource(driver, "MapAttribute");

        attribute.Should().Contain("internal sealed class MapAttribute : global::System.Attribute");
        return Task.CompletedTask;
    }

    [Fact]
    public Task EmitsSwitchBodyForMatchingProperties()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.MatchingProperties);
        return Verify(driver).UseFileName("MatchingProperties");
    }

    [Fact]
    public Task MatchesPropertiesCaseInsensitivelyAndAppliesImplicitConversions()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.CaseInsensitiveAndConvertible);
        return Verify(driver).UseFileName("CaseInsensitiveAndConvertible");
    }

    [Fact]
    public void SkipsUnmatchedAndUnconvertibleProperties()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.CaseInsensitiveAndConvertible);

        string generated = GetGeneratedSource(driver, "SourceMapper");

        generated.Should().Contain("NAME = source.Name,");
        generated.Should().Contain("Count = source.Count,");
        generated.Should().NotContain("Missing");
        generated.Should().NotContain("Ignored");
    }

    [Fact]
    public Task EmitsOneCasePerConcreteTarget()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.MultipleTargets);
        return Verify(driver).UseFileName("MultipleTargets");
    }

    [Fact]
    public void EmitsFallbackWhenMapperIsNeverInvoked()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.NoInvocations);

        string generated = GetGeneratedSource(driver, "WidgetMapper");

        generated.Should().Contain("throw new global::System.NotSupportedException(");
        generated.Should().NotContain("typeof(TTarget) ==");
    }

    [Fact]
    public void ReportsDiagnosticForUnmappableRequiredMember()
    {
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<MappingGenerator>(TestSources.UnmappableRequiredMember);

        Microsoft.CodeAnalysis.Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "MAP001").Subject;
        diagnostic.GetMessage().Should().Contain("Name");
    }

    #endregion

    #region Private Method Declarations

    private static string GetGeneratedSource(GeneratorDriver driver, string hintNameFragment)
    {
        SyntaxTree tree = driver.GetRunResult().GeneratedTrees
            .Single(generatedTree => generatedTree.FilePath.Contains(hintNameFragment));

        return tree.ToString();
    }

    #endregion
}