using FluentAssertions;
using ModularMonolith.Template.ComplianceGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ComplianceGen.Tests.Generators;

[Component(TestComponent.ComplianceGen)]
[Category(TestCategory.Generators)]
public sealed class ComplianceGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsRedactorForClassifiedStringMembers()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.SimpleProfile, TestSources.Abstractions);
        return Verify(driver).UseFileName("SimpleProfile");
    }

    [Fact]
    public void RedactsEachClassifiedMemberUsingResolvedRedactor()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.SimpleProfile, TestSources.Abstractions);

        string generated = GetGeneratedSource(driver, "CustomerClassification");

        generated.Should().Contain("global::ModularMonolith.Template.SharedKernel.Compliance.IModelRedactor<global::Contracts.Customer>");
        generated.Should().Contain("model.Name = redactorProvider.GetRedactor(global::ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal).Redact(model.Name);");
        generated.Should().Contain("model.Email = redactorProvider.GetRedactor(global::ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Sensitive).Redact(model.Email);");
        generated.Should().NotContain("Age");
    }

    [Fact]
    public void EmitsNullSafeGuardsForNestedMemberPaths()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.NestedProfile, TestSources.Abstractions);

        string generated = GetGeneratedSource(driver, "OrderClassification");

        generated.Should().Contain("if (model.ShipTo is not null)");
        generated.Should().Contain("model.ShipTo.PostCode = redactorProvider.GetRedactor(");
        generated.Should().Contain("model.Reference = redactorProvider.GetRedactor(");
    }

    [Fact]
    public void ReportsDiagnosticForNonStringMember()
    {
        System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<ComplianceGenerator>(TestSources.NonStringMember, TestSources.Abstractions);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "CMP002").Subject;
        diagnostic.GetMessage().Should().Contain("Balance");
    }

    [Fact]
    public void ReportsDiagnosticForUnsupportedSelector()
    {
        System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<ComplianceGenerator>(TestSources.UnsupportedSelector, TestSources.Abstractions);

        diagnostics.Should().ContainSingle(d => d.Id == "CMP001");
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
