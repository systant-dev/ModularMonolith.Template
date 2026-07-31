using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ModularMonolith.Template.MappingGen.Analysis;

/// <summary>
/// Formats symbols into the C# fragments (keys, type keywords, constraint clauses and
/// accessibility modifiers) used when emitting the generated mapper methods.
/// </summary>
internal static class SymbolFormatting
{
    #region Field Declarations

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    #endregion

    #region Internal Method Declarations

    public static string CreateKey(INamedTypeSymbol containingType, string methodName)
    {
        return $"{containingType.ToDisplayString()}.{methodName}";
    }

    public static string GetTypeKeyword(INamedTypeSymbol type)
    {
        if (type.IsRecord)
        {
            return type.TypeKind == TypeKind.Struct ? "record struct" : "record";
        }

        return type.TypeKind == TypeKind.Struct ? "struct" : "class";
    }

    public static string GetConstraintClause(ITypeParameterSymbol typeParameter)
    {
        List<string> constraints = [];

        if (typeParameter.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }
        else if (typeParameter.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }
        else if (typeParameter.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }
        else if (typeParameter.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
        {
            constraints.Add(constraintType.ToDisplayString(FullyQualifiedFormat));
        }

        if (typeParameter.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        if (constraints.Count == 0)
        {
            return string.Empty;
        }

        return $" where {typeParameter.Name} : {string.Join(", ", constraints)}";
    }

    public static string GetAccessibilityKeyword(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Private => "private",
            _ => "internal",
        };
    }

    #endregion
}
