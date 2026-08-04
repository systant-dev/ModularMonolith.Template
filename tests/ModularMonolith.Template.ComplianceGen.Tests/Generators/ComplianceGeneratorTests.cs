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

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "CustomerClassification");

        generated.Should().Contain("global::ModularMonolith.Template.SharedKernel.Compliance.Redaction.IModelRedactor<global::Contracts.Customer>");
        generated.Should().Contain("model.Name = redactorProvider.GetRedactor(global::ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal).Redact(model.Name);");
        generated.Should().Contain("model.Email = redactorProvider.GetRedactor(global::ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Sensitive).Redact(model.Email);");
        generated.Should().NotContain("Age");
    }

    [Fact]
    public void EmitsNullSafeGuardsForNestedMemberPaths()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.NestedProfile, TestSources.Abstractions);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "OrderClassification");

        generated.Should().Contain("if (model.ShipTo is not null)");
        generated.Should().Contain("model.ShipTo.PostCode = redactorProvider.GetRedactor(");
        generated.Should().Contain("model.Reference = redactorProvider.GetRedactor(");
    }

    [Fact]
    public void ReportsDiagnosticForNonStringMember()
    {
        System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<ComplianceGenerator>(TestSources.NonStringMember, TestSources.Abstractions);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "CMP002").Subject;
        diagnostic.GetMessage().Should().Contain("Balance");
    }

    [Fact]
    public void ReportsDiagnosticForUnsupportedSelector()
    {
        System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<ComplianceGenerator>(TestSources.UnsupportedSelector, TestSources.Abstractions);

        diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "CMP001");
    }

    [Fact]
    public Task EmitsRedactorForNestedMemberPath()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.NestedProfile, TestSources.Abstractions);
        return Verify(driver).UseFileName("NestedProfile");
    }

    [Fact]
    public void EmitsNothingWhenNoProfilesDeclared()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.NoProfiles, TestSources.Abstractions);

        driver.GetRunResult().GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void ProducesNoDiagnosticsWhenNoProfilesDeclared()
    {
        System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<ComplianceGenerator>(TestSources.NoProfiles, TestSources.Abstractions);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void EmitsOneRedactorPerProfile()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.MultipleProfiles, TestSources.Abstractions);

        string customer = GeneratorTestHelper.GetGeneratedSource(driver, "CustomerClassification");
        string supplier = GeneratorTestHelper.GetGeneratedSource(driver, "SupplierClassification");

        customer.Should().Contain("global::ModularMonolith.Template.SharedKernel.Compliance.Redaction.IModelRedactor<global::Contracts.Customer>");
        customer.Should().Contain("model.Name = redactorProvider.GetRedactor(");
        supplier.Should().Contain("global::ModularMonolith.Template.SharedKernel.Compliance.Redaction.IModelRedactor<global::Contracts.Supplier>");
        supplier.Should().Contain("model.ContactEmail = redactorProvider.GetRedactor(");
    }

    [Fact]
    public void EmitsRedactorForProfileInGlobalNamespace()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.GlobalNamespaceProfile, TestSources.Abstractions);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "AccountClassification");

        generated.Should().Contain("global::ModularMonolith.Template.SharedKernel.Compliance.Redaction.IModelRedactor<global::Account>");
        generated.Should().Contain("model.Owner = redactorProvider.GetRedactor(");
        generated.Should().NotContain("namespace");
    }

    [Fact]
    public void EmitsNullSafeGuardsForDeeplyNestedMemberPath()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.DeeplyNestedProfile, TestSources.Abstractions);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "CustomerClassification");

        generated.Should().Contain("if (model.HomeAddress is not null && model.HomeAddress.Country is not null)");
        generated.Should().Contain("model.HomeAddress.Country.IsoCode = redactorProvider.GetRedactor(");
    }

    [Fact]
    public void EmitsProfileAccessibilityAndTypeKeyword()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<ComplianceGenerator>(TestSources.InternalProfile, TestSources.Abstractions);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "TicketClassification");

        generated.Should().Contain("internal partial class TicketClassification");
        generated.Should().Contain("model.Subject = redactorProvider.GetRedactor(");
    }

    #endregion
}
