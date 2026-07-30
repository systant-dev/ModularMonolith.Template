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

    #endregion
}