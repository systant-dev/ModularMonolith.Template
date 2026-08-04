using FluentAssertions;
using ModularMonolith.Template.MappingGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.MappingGen.Tests.Generators;

[Component(TestComponent.MappingGen)]
[Category(TestCategory.Generators)]
public sealed class MappingGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsMapAttributeDuringPostInitialization()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.NoInvocations);

        string attribute = GeneratorTestHelper.GetGeneratedSource(driver, "MapAttribute");

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

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "SourceMapper");

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

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "WidgetMapper");

        generated.Should().Contain("throw new global::System.NotSupportedException(");
        generated.Should().NotContain("typeof(TTarget) ==");
    }

    [Fact]
    public void EmitsDirectBodyForConcreteReturnType()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.ConcreteReturnType);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "ProductMapper");

        generated.Should().Contain("static partial global::Contracts.ProductDto ToDto(this global::Contracts.Product source)");
        generated.Should().Contain("return new global::Contracts.ProductDto");
        generated.Should().Contain("Id = source.Id,");
        generated.Should().Contain("Name = source.Name,");
        generated.Should().NotContain("typeof(");
        generated.Should().NotContain("NotSupportedException");
    }

    [Fact]
    public void ReportsDiagnosticForUnmappableRequiredMember()
    {
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<MappingGenerator>(TestSources.UnmappableRequiredMember);

        Microsoft.CodeAnalysis.Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "MAP001").Subject;
        diagnostic.GetMessage().Should().Contain("Name");
    }

    [Fact]
    public void AppliesToStringAndExplicitNumericAndEnumConversions()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.TypeConversions);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "ConversionMapper");

        generated.Should().Contain("Code = source.Code.ToString(),");
        generated.Should().Contain("Big = (int)(source.Big),");
        generated.Should().Contain("Shade = (int)(source.Shade),");
    }

    [Fact]
    public void MapsNestedReferenceTypeRecursively()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.NestedMapping);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "PersonMapper");

        generated.Should().Contain("HomeAddress = source.HomeAddress is null ? null : new global::Contracts.AddressDto");
        generated.Should().Contain("City = source.HomeAddress.City");
        generated.Should().Contain("Zip = source.HomeAddress.Zip");
    }

    [Fact]
    public void MapsCollectionElementsAndMaterialisesToArray()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.CollectionMapping);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "BasketMapper");

        generated.Should().Contain("global::System.Linq.Enumerable.ToArray(");
        generated.Should().Contain("global::System.Linq.Enumerable.Select(source.Items, item =>");
        generated.Should().Contain("new global::Contracts.ItemDto");
    }

    [Fact]
    public void ReportsDiagnosticForRecursiveMappingCycle()
    {
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<MappingGenerator>(TestSources.RecursiveCycle);

        Microsoft.CodeAnalysis.Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "MAP003").Subject;
        diagnostic.GetMessage().Should().Contain("Next");
    }

    [Fact]
    public void ReportsDiagnosticForIncompatibleOptionalMember()
    {
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            GeneratorTestHelper.RunGeneratorDiagnostics<MappingGenerator>(TestSources.IncompatibleOptionalMember);

        Microsoft.CodeAnalysis.Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "MAP002").Subject;
        diagnostic.GetMessage().Should().Contain("Amount");
    }

    [Fact]
    public void MapsNestedComplexCollectionToList()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.NestedCollectionMapping);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "InvoiceMapper");

        generated.Should().Contain("global::System.Linq.Enumerable.ToList(");
        generated.Should().Contain("new global::Contracts.LineDto");
        generated.Should().Contain("new global::Contracts.OrderDto");
    }

    [Fact]
    public void MapsPropertiesInheritedFromBaseClass()
    {
        GeneratorDriver driver = GeneratorTestHelper.RunGenerator<MappingGenerator>(TestSources.InheritedProperties);

        string generated = GeneratorTestHelper.GetGeneratedSource(driver, "EmployeeMapper");

        generated.Should().Contain("Id = source.Id,");
        generated.Should().Contain("Name = source.Name,");
        generated.Should().Contain("Department = source.Department,");
    }

    #endregion
}