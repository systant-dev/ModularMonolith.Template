using Microsoft.CodeAnalysis;

namespace ModularMonolith.Template.MappingGen.Diagnostics;

internal static class MappingDiagnostics
{
    #region Field Declarations

    public static readonly DiagnosticDescriptor MissingRequiredMember = new(
        id: "MAP001",
        title: "Mapping cannot satisfy required members",
        messageFormat: "Cannot map '{0}' to '{1}' because the required member(s) {2} have no matching source property",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every required member on the target type must have a matching, convertible public property on the source type.");

    public static readonly DiagnosticDescriptor IncompatibleMemberType = new(
        id: "MAP002",
        title: "Matching member skipped due to incompatible type",
        messageFormat: "Property '{0}' was not mapped from '{1}' to '{2}' because source type '{3}' is not implicitly convertible to target type '{4}'",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A source and target property share a name but their types are not identity- or implicitly-convertible, so the property is left unmapped.");

    public static readonly DiagnosticDescriptor RecursiveMappingCycle = new(
        id: "MAP003",
        title: "Recursive mapping cycle detected",
        messageFormat: "Property '{0}' was not mapped because a recursive mapping cycle was detected while mapping type '{1}'",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Nested mapping stops when a target type is already being expanded on the current mapping path to avoid infinite recursion; the offending property is left unmapped.");

    #endregion
}