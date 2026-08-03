using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModularMonolith.Template.MappingGen.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Analysis;

internal static class MappingResolver
{
    #region Field Declarations

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    #endregion

    #region Public Method Declarations

    public static MappingResolution Resolve(INamedTypeSymbol sourceType, INamedTypeSymbol targetType, string sourceAccessor, Compilation compilation)
    {
        Dictionary<string, IPropertySymbol> sourceProperties = SymbolInspection.GetReadableProperties(sourceType);
        ImmutableArray<MapPropertyAssignment>.Builder builder = ImmutableArray.CreateBuilder<MapPropertyAssignment>();
        ImmutableArray<string>.Builder unmappedRequired = ImmutableArray.CreateBuilder<string>();
        List<MapMemberTypeMismatch> incompatible = [];
        List<MapCycleMember> cycles = [];

        ImmutableHashSet<string> visited = ImmutableHashSet.Create(StringComparer.Ordinal, targetType.ToDisplayString(FullyQualifiedFormat));

        foreach (IPropertySymbol targetProperty in SymbolInspection.GetWritableProperties(targetType))
        {
            bool mapped = false;

            if (sourceProperties.TryGetValue(targetProperty.Name.ToLowerInvariant(), out IPropertySymbol sourceProperty))
            {
                string memberAccessor = $"{sourceAccessor}.{sourceProperty.Name}";
                (MapOutcome outcome, string? expression) = BuildValueExpression(sourceProperty.Type, targetProperty.Type, memberAccessor, compilation, visited, incompatible, cycles);

                switch (outcome)
                {
                    case MapOutcome.Mapped:
                        MapPropertyAssignment assignment = new(targetProperty.Name, expression!);
                        builder.Add(assignment);
                        mapped = true;
                        break;

                    case MapOutcome.Cycle:
                        MapCycleMember cycleMember = new(targetProperty.Name, targetProperty.Type.ToDisplayString());
                        cycles.Add(cycleMember);
                        break;

                    default:
                        MapMemberTypeMismatch mismatch = new(targetProperty.Name, sourceProperty.Type.ToDisplayString(), targetProperty.Type.ToDisplayString());
                        incompatible.Add(mismatch);
                        break;
                }
            }

            if (!mapped && targetProperty.IsRequired)
            {
                unmappedRequired.Add(targetProperty.Name);
            }
        }

        MappingResolution mappingResolution = new(builder.ToImmutable(), unmappedRequired.ToImmutable(), [.. incompatible], [.. cycles]);
        return mappingResolution;
    }

    #endregion

    #region Private Method Declarations

    private static (MapOutcome Outcome, string? Expression) BuildValueExpression(ITypeSymbol sourceType, ITypeSymbol targetType, string accessor, Compilation compilation, ImmutableHashSet<string> visited, List<MapMemberTypeMismatch> incompatible, List<MapCycleMember> cycles)
    {
        Conversion conversion = compilation.ClassifyConversion(sourceType, targetType);
        if (conversion.IsIdentity || conversion.IsImplicit)
        {
            return (MapOutcome.Mapped, accessor);
        }

        if (targetType.SpecialType == SpecialType.System_String && sourceType.SpecialType != SpecialType.System_String)
        {
            string toString = SymbolInspection.CanBeNull(sourceType) ? $"{accessor}?.ToString()" : $"{accessor}.ToString()";
            return (MapOutcome.Mapped, toString);
        }

        if (conversion.IsExplicit && SymbolInspection.IsNumericOrEnum(sourceType) && SymbolInspection.IsNumericOrEnum(targetType))
        {
            return (MapOutcome.Mapped, $"({targetType.ToDisplayString(FullyQualifiedFormat)})({accessor})");
        }

        if (SymbolInspection.TryGetElementType(sourceType, out ITypeSymbol? sourceElement)
            && SymbolInspection.TryGetTargetCollection(targetType, out ITypeSymbol? targetElement, out bool targetIsArray))
        {
            (MapOutcome elementOutcome, string? elementExpression) = BuildValueExpression(sourceElement!, targetElement!, "item", compilation, visited, incompatible, cycles);
            if (elementOutcome != MapOutcome.Mapped)
            {
                return (elementOutcome, null);
            }

            string projected = elementExpression == "item"
                ? accessor
                : $"global::System.Linq.Enumerable.Select({accessor}, item => {elementExpression})";
            string materialised = targetIsArray
                ? $"global::System.Linq.Enumerable.ToArray({projected})"
                : $"global::System.Linq.Enumerable.ToList({projected})";
            return (MapOutcome.Mapped, $"{accessor} is null ? null : {materialised}");
        }

        if (sourceType is INamedTypeSymbol namedSource && targetType is INamedTypeSymbol namedTarget
            && SymbolInspection.IsUserMappable(namedSource) && SymbolInspection.IsUserMappable(namedTarget))
        {
            string targetKey = namedTarget.ToDisplayString(FullyQualifiedFormat);
            if (visited.Contains(targetKey))
            {
                return (MapOutcome.Cycle, null);
            }

            string? initializer = TryBuildObjectInitializer(namedSource, namedTarget, accessor, compilation, visited.Add(targetKey), incompatible, cycles);
            return initializer is null ? (MapOutcome.Incompatible, null) : (MapOutcome.Mapped, initializer);
        }

        return (MapOutcome.Incompatible, null);
    }

    private static string? TryBuildObjectInitializer(INamedTypeSymbol sourceType, INamedTypeSymbol targetType, string accessor, Compilation compilation, ImmutableHashSet<string> visited, List<MapMemberTypeMismatch> incompatible, List<MapCycleMember> cycles)
    {
        Dictionary<string, IPropertySymbol> sourceProperties = SymbolInspection.GetReadableProperties(sourceType);
        List<string> assignments = [];

        foreach (IPropertySymbol targetProperty in SymbolInspection.GetWritableProperties(targetType))
        {
            if (!sourceProperties.TryGetValue(targetProperty.Name.ToLowerInvariant(), out IPropertySymbol sourceProperty))
            {
                if (targetProperty.IsRequired)
                {
                    return null;
                }

                continue;
            }

            string memberAccessor = $"{accessor}.{sourceProperty.Name}";
            (MapOutcome outcome, string? expression) = BuildValueExpression(sourceProperty.Type, targetProperty.Type, memberAccessor, compilation, visited, incompatible, cycles);

            switch (outcome)
            {
                case MapOutcome.Mapped:
                    assignments.Add($"{targetProperty.Name} = {expression}");
                    break;

                case MapOutcome.Cycle:
                    MapCycleMember cycleMember = new(targetProperty.Name, targetProperty.Type.ToDisplayString());
                    cycles.Add(cycleMember);
                    if (targetProperty.IsRequired)
                    {
                        return null;
                    }

                    break;

                default:
                    MapMemberTypeMismatch mismatch = new(
                        targetProperty.Name,
                        sourceProperty.Type.ToDisplayString(),
                        targetProperty.Type.ToDisplayString());
                    incompatible.Add(mismatch);
                    if (targetProperty.IsRequired)
                    {
                        return null;
                    }

                    break;
            }
        }

        string typeName = targetType.ToDisplayString(FullyQualifiedFormat);
        string body = assignments.Count == 0 ? $"new {typeName}()" : $"new {typeName} {{ {string.Join(", ", assignments)} }}";
        return SymbolInspection.CanBeNull(sourceType) ? $"{accessor} is null ? null : {body}" : body;
    }

    #endregion
}