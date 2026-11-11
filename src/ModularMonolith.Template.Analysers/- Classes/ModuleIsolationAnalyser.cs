using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class ModuleIsolationAnalyser : DiagnosticAnalyzer
{
    #region Field Declarations

    public const string DiagnosticId = "MM0002";

    private const string ModuleSuffix = "Module";

    private const string ContractsSegment = "Contracts";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Modules must not reference another module's internals",
        messageFormat: "Type '{0}' belongs to module '{1}'; modules may only reference another module through its '{1}.Contracts' namespace",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A module may only depend on another module through its public Contracts namespace. Referencing another module's internal types couples modules and undermines the modular boundary.");

    #endregion

    #region Property Declarations

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    #endregion

    #region Public Method Declarations

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(
            AnalyseOperation,
            OperationKind.ObjectCreation,
            OperationKind.Invocation,
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.MethodReference,
            OperationKind.VariableDeclarator,
            OperationKind.TypeOf,
            OperationKind.IsType,
            OperationKind.Conversion);
        context.RegisterSymbolAction(AnalyseSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property);
    }

    #endregion

    #region Private Method Declarations

    private static void AnalyseOperation(OperationAnalysisContext context)
    {
        string? currentModule = GetModuleRoot(context.ContainingSymbol?.ContainingNamespace);

        if (currentModule is null)
        {
            return;
        }

        Location location = context.Operation.Syntax.GetLocation();

        foreach (ITypeSymbol referenced in GetReferencedTypes(context.Operation))
        {
            Report(context.ReportDiagnostic, referenced, currentModule, location);
        }
    }

    private static void AnalyseSymbol(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;

        if (symbol.IsImplicitlyDeclared || symbol.Locations.IsDefaultOrEmpty)
        {
            return;
        }

        string? currentModule = GetModuleRoot(symbol.ContainingNamespace);

        if (currentModule is null)
        {
            return;
        }

        Location location = symbol.Locations[0];

        switch (symbol)
        {
            case INamedTypeSymbol namedType:
                Report(context.ReportDiagnostic, namedType.BaseType, currentModule, location);

                foreach (INamedTypeSymbol implemented in namedType.Interfaces)
                {
                    Report(context.ReportDiagnostic, implemented, currentModule, location);
                }

                break;

            case IMethodSymbol { MethodKind: not (MethodKind.PropertyGet or MethodKind.PropertySet) } method:
                Report(context.ReportDiagnostic, method.ReturnType, currentModule, location);

                foreach (IParameterSymbol parameter in method.Parameters)
                {
                    Location parameterLocation = parameter.Locations.IsDefaultOrEmpty ? location : parameter.Locations[0];
                    Report(context.ReportDiagnostic, parameter.Type, currentModule, parameterLocation);
                }

                break;

            case IFieldSymbol field:
                Report(context.ReportDiagnostic, field.Type, currentModule, location);
                break;

            case IPropertySymbol property:
                Report(context.ReportDiagnostic, property.Type, currentModule, location);
                break;
        }
    }

    private static IEnumerable<ITypeSymbol> GetReferencedTypes(IOperation operation)
    {
        switch (operation)
        {
            case IObjectCreationOperation { Constructor: { } constructor }:
                yield return constructor.ContainingType;
                break;

            case IInvocationOperation invocation:
                yield return invocation.TargetMethod.ContainingType;

                foreach (ITypeSymbol typeArgument in invocation.TargetMethod.TypeArguments)
                {
                    yield return typeArgument;
                }

                break;

            case IFieldReferenceOperation fieldReference:
                yield return fieldReference.Field.ContainingType;
                break;

            case IPropertyReferenceOperation propertyReference:
                yield return propertyReference.Property.ContainingType;
                break;

            case IMethodReferenceOperation methodReference:
                yield return methodReference.Method.ContainingType;
                break;

            case IVariableDeclaratorOperation variableDeclarator:
                yield return variableDeclarator.Symbol.Type;
                break;

            case ITypeOfOperation typeOf:
                yield return typeOf.TypeOperand;
                break;

            case IIsTypeOperation isType:
                yield return isType.TypeOperand;
                break;

            case IConversionOperation { IsImplicit: false, Type: { } convertedType }:
                yield return convertedType;
                break;
        }
    }

    private static void Report(Action<Diagnostic> report, ITypeSymbol? type, string currentModule, Location location)
    {
        switch (type)
        {
            case IArrayTypeSymbol arrayType:
                Report(report, arrayType.ElementType, currentModule, location);
                return;

            case INamedTypeSymbol namedType:
                foreach (ITypeSymbol typeArgument in namedType.TypeArguments)
                {
                    Report(report, typeArgument, currentModule, location);
                }

                string? referencedModule = GetModuleRoot(namedType.ContainingNamespace);

                if (referencedModule is null || referencedModule == currentModule || IsContractsNamespace(namedType.ContainingNamespace, referencedModule))
                {
                    return;
                }

                report(Diagnostic.Create(Rule, location, namedType.Name, referencedModule));
                return;
        }
    }

    private static string? GetModuleRoot(INamespaceSymbol? namespaceSymbol)
    {
        string? root = null;

        for (INamespaceSymbol? current = namespaceSymbol; current is { IsGlobalNamespace: false }; current = current.ContainingNamespace)
        {
            root = current.Name;
        }

        return root is not null && root.EndsWith(ModuleSuffix, StringComparison.Ordinal) ? root : null;
    }

    private static bool IsContractsNamespace(INamespaceSymbol? namespaceSymbol, string moduleRoot)
    {
        bool sawContracts = false;
        bool sawRoot = false;

        for (INamespaceSymbol? current = namespaceSymbol; current is { IsGlobalNamespace: false }; current = current.ContainingNamespace)
        {
            if (current.Name == ContractsSegment && current.ContainingNamespace?.Name == moduleRoot)
            {
                sawContracts = true;
            }

            if (current.Name == moduleRoot)
            {
                sawRoot = true;
            }
        }

        return sawContracts && sawRoot;
    }

    #endregion
}
