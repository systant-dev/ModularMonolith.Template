using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class ModuleAttributeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MM0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Module attribute must be applied to types implementing IModule",
        messageFormat: "Type '{0}' is marked with [Module] but does not implement IModule",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types annotated with ModularMonolith.Template.SharedKernel.Modular.ModuleAttribute must implement the IModule interface.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;
        ImmutableArray<AttributeData> attrs = namedType.GetAttributes();

        foreach (AttributeData attr in attrs)
        {
            if (attr.AttributeClass?.ToDisplayString() == "ModularMonolith.Template.SharedKernel.Modular.ModuleAttribute")
            {
                Compilation compilation = context.Compilation;
                INamedTypeSymbol? moduleInterface = compilation.GetTypeByMetadataName("ModularMonolith.Template.SharedKernel.Modular.IModule");

                bool implementsInterface = false;

                if (moduleInterface != null)
                {
                    implementsInterface = namedType.AllInterfaces.Contains(moduleInterface, SymbolEqualityComparer.Default);
                }
                else
                {
                    implementsInterface = namedType.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.Name == "IModule");
                }

                if (!implementsInterface)
                {
                    Location? location = attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? namedType.Locations.FirstOrDefault();
                    Diagnostic diagnostic = Diagnostic.Create(Rule, location ?? namedType.Locations.First(), namedType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
