using Microsoft.CodeAnalysis;
using ModularMonolith.Template.ModularGen.Analysis;
using ModularMonolith.Template.ModularGen.Diagnostics;
using ModularMonolith.Template.ModularGen.Emit;
using ModularMonolith.Template.SourceGenerators.Shared.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.ModularGen.Generators;

[Generator]
public sealed class ModuleRegistrationGenerator : IIncrementalGenerator
{
    #region Field Declarations

    private const string ModuleAttributeName = "ModularMonolith.Template.SharedKernel.Modular.ModuleAttribute";

    private const string ModuleInterfaceName = "ModularMonolith.Template.SharedKernel.Modular.IModule";

    #endregion

    #region Public Method Declarations

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<ModuleCandidate>> moduleTypesProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ModuleAttributeName,
                predicate: static (_, _) => true,
                transform: static (attributeContext, _) => GetModuleCandidate(attributeContext))
            .Where(static candidate => candidate is not null)
            .Select(static (candidate, _) => candidate!.Value)
            .Collect()
            .WithTrackingName("ModuleRegistrations");

        context.RegisterSourceOutput(moduleTypesProvider, static (sourceContext, candidates) =>
        {
            foreach (ModuleCandidate candidate in candidates)
            {
                if (candidate.TypeName is null)
                {
                    Diagnostic moduleNotInstantiable = Diagnostic.Create(
                        ModularDiagnostics.ModuleNotInstantiable,
                        candidate.Location?.ToLocation(),
                        candidate.DisplayName);
                    sourceContext.ReportDiagnostic(moduleNotInstantiable);
                }
            }

            ImmutableArray<string> moduleTypes = [.. candidates
                .Where(candidate => candidate.TypeName is not null)
                .Select(candidate => candidate.TypeName!)];

            if (moduleTypes.IsDefaultOrEmpty)
            {
                return;
            }

            string source = GenerateAssemblyAttributes(moduleTypes);
            sourceContext.AddSource("ModuleRegistrations.g.cs", source);
        });
    }

    #endregion

    #region Private Method Declarations

    private static ModuleCandidate? GetModuleCandidate(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol type)
        {
            return null;
        }

        LocationInfo? location = LocationInfo.CreateFrom(context.TargetNode);
        string displayName = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        INamedTypeSymbol? moduleInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(ModuleInterfaceName);
        if (moduleInterface is null || !ModuleSymbolAnalyser.IsInstantiableModule(type, moduleInterface))
        {
            return new ModuleCandidate(null, displayName, location);
        }

        return new ModuleCandidate(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), displayName, location);
    }

    private static string GenerateAssemblyAttributes(ImmutableArray<string> moduleTypes)
    {
        return ModuleRegistrationEmitter.Emit(moduleTypes);
    }

    #endregion

    #region Nested Types

    private readonly record struct ModuleCandidate(string? TypeName, string DisplayName, LocationInfo? Location);

    #endregion
}