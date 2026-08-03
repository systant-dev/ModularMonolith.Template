using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class ModuleAttributeAnalyser : DiagnosticAnalyzer
{
    #region Field Declarations

    public const string DiagnosticId = "MM0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Module attribute must be applied to types implementing IModule",
        messageFormat: "Type '{0}' is marked with [Module] but does not implement IModule",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types annotated with ModularMonolith.Template.SharedKernel.Modular.ModuleAttribute must implement the IModule interface.");

    #endregion

    #region Property Declarations

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    #endregion

    #region Public Method Declarations

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyseNamedType, SymbolKind.NamedType);
    }

    #endregion

    #region Private Method Declarations

    private static void AnalyseNamedType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;
        ImmutableArray<AttributeData> attributes = namedType.GetAttributes();

        foreach (AttributeData attribute in attributes)
        {
            if (attribute.AttributeClass?.ToDisplayString() == "ModularMonolith.Template.SharedKernel.Modular.ModuleAttribute")
            {
                Compilation compilation = context.Compilation;
                INamedTypeSymbol? moduleInterface = compilation.GetTypeByMetadataName("ModularMonolith.Template.SharedKernel.Modular.IModule");

                bool implementsInterface = false;

                if (moduleInterface is not null)
                {
                    implementsInterface = namedType.AllInterfaces.Contains(moduleInterface, SymbolEqualityComparer.Default);
                }
                else
                {
                    implementsInterface = namedType.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.Name == "IModule");
                }

                if (!implementsInterface)
                {
                    Location? location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? namedType.Locations.FirstOrDefault();
                    Diagnostic diagnostic = Diagnostic.Create(Rule, location ?? namedType.Locations.First(), namedType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    #endregion
}