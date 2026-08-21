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
        context.RegisterOperationAction(AnalyseOperation, OperationKind.ObjectCreation, OperationKind.Invocation, OperationKind.FieldReference, OperationKind.PropertyReference, OperationKind.MethodReference);
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

        ISymbol? referenced = context.Operation switch
        {
            IObjectCreationOperation objectCreation => objectCreation.Constructor?.ContainingType,
            IInvocationOperation invocation => invocation.TargetMethod.ContainingType,
            IFieldReferenceOperation fieldReference => fieldReference.Field.ContainingType,
            IPropertyReferenceOperation propertyReference => propertyReference.Property.ContainingType,
            IMethodReferenceOperation methodReference => methodReference.Method.ContainingType,
            _ => null,
        };

        if (referenced is not INamedTypeSymbol referencedType)
        {
            return;
        }

        string? referencedModule = GetModuleRoot(referencedType.ContainingNamespace);

        if (referencedModule is null || referencedModule == currentModule)
        {
            return;
        }

        if (IsContractsNamespace(referencedType.ContainingNamespace, referencedModule))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), referencedType.Name, referencedModule);
        context.ReportDiagnostic(diagnostic);
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
