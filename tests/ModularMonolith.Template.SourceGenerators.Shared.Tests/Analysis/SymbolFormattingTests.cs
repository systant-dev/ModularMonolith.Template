using FluentAssertions;
using Microsoft.CodeAnalysis;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Traits;

namespace ModularMonolith.Template.SourceGenerators.Shared.Tests.Analysis;

[Component(TestComponent.SourceGeneratorsShared)]
[Category(TestCategory.Generators)]
public sealed class SymbolFormattingTests
{
    #region Test Method Declarations

    [Fact]
    public void CreateKey_CombinesContainingTypeDisplayAndMethodName()
    {
        INamedTypeSymbol type = GetNamedType("namespace Sample { public class Widget { } }", "Sample.Widget");

        string key = SymbolFormatting.CreateKey(type, "Build");

        key.Should().Be("Sample.Widget.Build");
    }

    [Theory]
    [InlineData("public class Target { }", "class")]
    [InlineData("public struct Target { }", "struct")]
    [InlineData("public record Target { }", "record")]
    [InlineData("public record struct Target { }", "record struct")]
    public void GetTypeKeyword_ReturnsExpectedKeyword(string declaration, string expected)
    {
        INamedTypeSymbol type = GetNamedType($"namespace Sample {{ {declaration} }}", "Sample.Target");

        SymbolFormatting.GetTypeKeyword(type).Should().Be(expected);
    }

    [Theory]
    [InlineData(Accessibility.Public, "public")]
    [InlineData(Accessibility.Internal, "internal")]
    [InlineData(Accessibility.Protected, "protected")]
    [InlineData(Accessibility.ProtectedOrInternal, "protected internal")]
    [InlineData(Accessibility.ProtectedAndInternal, "private protected")]
    [InlineData(Accessibility.Private, "private")]
    [InlineData(Accessibility.NotApplicable, "internal")]
    public void GetAccessibilityKeyword_ReturnsExpectedKeyword(Accessibility accessibility, string expected)
    {
        SymbolFormatting.GetAccessibilityKeyword(accessibility).Should().Be(expected);
    }

    [Fact]
    public void GetConstraintClause_ReturnsEmpty_WhenNoConstraints()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().BeEmpty();
    }

    [Fact]
    public void GetConstraintClause_EmitsClass_ForReferenceTypeConstraint()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : class { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : class");
    }

    [Fact]
    public void GetConstraintClause_EmitsStruct_ForValueTypeConstraint()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : struct { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : struct");
    }

    [Fact]
    public void GetConstraintClause_EmitsUnmanaged_ForUnmanagedConstraint()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : unmanaged { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : unmanaged");
    }

    [Fact]
    public void GetConstraintClause_EmitsNotNull_ForNotNullConstraint()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : notnull { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : notnull");
    }

    [Fact]
    public void GetConstraintClause_EmitsNewConstructorConstraint()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : new() { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : new()");
    }

    [Fact]
    public void GetConstraintClause_EmitsFullyQualifiedConstraintType()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : System.IComparable { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : global::System.IComparable");
    }

    [Fact]
    public void GetConstraintClause_CombinesConstraintsInDeclarationOrder()
    {
        ITypeParameterSymbol typeParameter = GetTypeParameter("public class Holder<T> where T : class, System.IComparable, new() { }");

        SymbolFormatting.GetConstraintClause(typeParameter).Should().Be(" where T : class, global::System.IComparable, new()");
    }

    #endregion

    #region Private Method Declarations

    private static INamedTypeSymbol GetNamedType(string source, string metadataName)
    {
        Compilation compilation = GeneratorTestHelper.CreateCompilation(source);
        INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(metadataName);
        symbol.Should().NotBeNull();
        return symbol!;
    }

    private static ITypeParameterSymbol GetTypeParameter(string declaration)
    {
        INamedTypeSymbol type = GetNamedType($"namespace Sample {{ {declaration} }}", "Sample.Holder`1");
        return type.TypeParameters[0];
    }

    #endregion
}
