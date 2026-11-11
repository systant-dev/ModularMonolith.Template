using Microsoft.CodeAnalysis;

namespace ModularMonolith.Template.ModularGen.Diagnostics;

internal static class ModularDiagnostics
{
    #region Field Declarations

    public static readonly DiagnosticDescriptor MultipleRegistrationTargets = new(
        id: "MOD001",
        title: "Multiple module registration targets",
        messageFormat: "Multiple methods are annotated with [ModuleRegistrationTarget]; only '{0}' will be generated. Remove the extra targets so exactly one exists.",
        category: "Modular",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Exactly one partial method should be annotated with [ModuleRegistrationTarget] so module registration is generated deterministically.");

    public static readonly DiagnosticDescriptor ModuleNotInstantiable = new(
        id: "MOD002",
        title: "Module type is not instantiable",
        messageFormat: "Type '{0}' is annotated with [Module] but is not an instantiable module; it must be a non-abstract, non-static class that implements IModule and has a public parameterless constructor",
        category: "Modular",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A [Module] type must be a non-abstract, non-static class implementing IModule with a public parameterless constructor to be registered.");

    #endregion
}
