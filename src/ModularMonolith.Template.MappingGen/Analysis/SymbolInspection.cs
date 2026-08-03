using Microsoft.CodeAnalysis;

namespace ModularMonolith.Template.MappingGen.Analysis;

internal static class SymbolInspection
{
    #region Public Method Declarations

    public static bool CanBeNull(ITypeSymbol type)
    {
        return type.IsReferenceType || (type is INamedTypeSymbol named && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T);
    }

    public static bool IsNumericOrEnum(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        return type.SpecialType switch
        {
            SpecialType.System_Char
            or SpecialType.System_SByte
            or SpecialType.System_Byte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal => true,
            _ => false,
        };
    }

    public static bool TryGetElementType(ITypeSymbol type, out ITypeSymbol? elementType)
    {
        elementType = null;

        if (type.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        if (type is IArrayTypeSymbol { Rank: 1 } array)
        {
            elementType = array.ElementType;
            return true;
        }

        if (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            elementType = named.TypeArguments[0];
            return true;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                elementType = iface.TypeArguments[0];
                return true;
            }
        }

        return false;
    }

    public static bool TryGetTargetCollection(ITypeSymbol type, out ITypeSymbol? elementType, out bool isArray)
    {
        elementType = null;
        isArray = false;

        if (type.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        if (type is IArrayTypeSymbol { Rank: 1 } array)
        {
            elementType = array.ElementType;
            isArray = true;
            return true;
        }

        if (type is INamedTypeSymbol { IsGenericType: true } named)
        {
            switch (named.ConstructedFrom.ToDisplayString())
            {
                case "System.Collections.Generic.List<T>":
                case "System.Collections.Generic.IList<T>":
                case "System.Collections.Generic.ICollection<T>":
                case "System.Collections.Generic.IEnumerable<T>":
                case "System.Collections.Generic.IReadOnlyList<T>":
                case "System.Collections.Generic.IReadOnlyCollection<T>":
                    elementType = named.TypeArguments[0];
                    return true;
            }
        }

        return false;
    }

    public static bool IsUserMappable(INamedTypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None || type.IsAnonymousType || type.IsAbstract)
        {
            return false;
        }

        if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
        {
            return false;
        }

        string containingNamespace = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (containingNamespace == "System" || containingNamespace.StartsWith("System.", System.StringComparison.Ordinal))
        {
            return false;
        }

        if (type.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        foreach (IMethodSymbol constructor in type.InstanceConstructors)
        {
            if (constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public)
            {
                return true;
            }
        }

        return false;
    }

    public static Dictionary<string, IPropertySymbol> GetReadableProperties(INamedTypeSymbol type)
    {
        Dictionary<string, IPropertySymbol> properties = new(System.StringComparer.Ordinal);

        for (INamedTypeSymbol? current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is not IPropertySymbol property
                    || property.DeclaredAccessibility != Accessibility.Public
                    || property.IsStatic
                    || property.IsIndexer
                    || property.GetMethod is null
                    || property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                string key = property.Name.ToLowerInvariant();
                if (!properties.ContainsKey(key))
                {
                    properties.Add(key, property);
                }
            }
        }

        return properties;
    }

    public static IEnumerable<IPropertySymbol> GetWritableProperties(INamedTypeSymbol type)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);

        for (INamedTypeSymbol? current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is not IPropertySymbol property
                    || property.DeclaredAccessibility != Accessibility.Public
                    || property.IsStatic
                    || property.IsIndexer
                    || property.SetMethod is null
                    || property.SetMethod.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (seen.Add(property.Name))
                {
                    yield return property;
                }
            }
        }
    }

    #endregion
}