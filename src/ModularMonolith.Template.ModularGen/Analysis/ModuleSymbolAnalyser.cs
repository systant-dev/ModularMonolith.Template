using Microsoft.CodeAnalysis;

namespace ModularMonolith.Template.ModularGen.Analysis;

internal static class ModuleSymbolAnalyser
{
    #region Public Method Declarations

    public static bool IsInstantiableModule(INamedTypeSymbol type, INamedTypeSymbol moduleInterface)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
        {
            return false;
        }
        if (!type.AllInterfaces.Contains(moduleInterface, SymbolEqualityComparer.Default))
        {
            return false;
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

    #endregion
}