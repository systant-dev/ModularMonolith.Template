using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModularMonolith.Template.ModularGen.Analysis;
using ModularMonolith.Template.ModularGen.Diagnostics;
using ModularMonolith.Template.ModularGen.Emit;
using ModularMonolith.Template.SourceGenerators.Shared.Collections;
using ModularMonolith.Template.SourceGenerators.Shared.Models;
using ModularMonolith.Template.ModularGen.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.ModularGen.Generators;

[Generator]
public sealed class ModuleEndpointGenerator : IIncrementalGenerator
{
    #region Field Declarations

    private const string ModuleRegistrationAttributeName = "ModularMonolith.Template.SharedKernel.Modular.ModuleRegistrationAttribute`1";
    private const string ModuleInterfaceName = "ModularMonolith.Template.SharedKernel.Modular.IModule";
    private const string ModuleEndpointMappingTargetAttributeName = "ModularMonolith.Template.SharedKernel.Modular.ModuleEndpointMappingTargetAttribute";

    #endregion

    #region Public Method Declarations

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StubInfo>> stubProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ModuleEndpointMappingTargetAttributeName,
                predicate: static (node, _) => IsPartialStubMethod(node),
                transform: static (attributeContext, cancellationToken) => GetStubInfo(attributeContext))
            .Where(static stub => stub is not null)
            .Select(static (stub, _) => stub!.Value)
            .Collect();

        IncrementalValueProvider<EquatableArray<ModuleInfo>> modulesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => GetRegisteredModules(compilation, cancellationToken))
            .WithTrackingName("GetEndpointModules");

        context.RegisterSourceOutput(stubProvider.Combine(modulesProvider), static (sourceContext, pair) =>
        {
            ImmutableArray<StubInfo> stubs = pair.Left;
            if (stubs.IsDefaultOrEmpty)
            {
                return;
            }

            ImmutableArray<StubInfo> orderedStubs = [.. stubs
                .OrderBy(stub => stub.Namespace, StringComparer.Ordinal)
                .ThenBy(stub => stub.ClassName, StringComparer.Ordinal)
                .ThenBy(stub => stub.MethodName, StringComparer.Ordinal)];

            StubInfo stub = orderedStubs[0];

            for (int index = 1; index < orderedStubs.Length; index++)
            {
                Diagnostic multipleRegistrationTargets = Diagnostic.Create(
                    ModularDiagnostics.MultipleRegistrationTargets,
                    orderedStubs[index].Location?.ToLocation(),
                    $"{stub.ClassName}.{stub.MethodName}");
                sourceContext.ReportDiagnostic(multipleRegistrationTargets);
            }

            string source = GenerateModuleEndpointExtensions(stub, pair.Right);
            sourceContext.AddSource("ModuleEndpointRouteBuilderExtensions.g.cs", source);
        });
    }

    #endregion

    #region Private Method Declarations

    private static bool IsPartialStubMethod(SyntaxNode node)
    {
        return node is MethodDeclarationSyntax method
            && method.Modifiers.Any(SyntaxKind.PartialKeyword)
            && method.Parent is TypeDeclarationSyntax;
    }

    private static StubInfo? GetStubInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        INamedTypeSymbol containingType = methodSymbol.ContainingType;
        string namespaceName = containingType.ContainingNamespace.ToDisplayString();
        LocationInfo? location = context.TargetNode is { } node ? LocationInfo.CreateFrom(node) : null;
        return new StubInfo(namespaceName, containingType.Name, methodSymbol.Name, location);
    }

    private static EquatableArray<ModuleInfo> GetRegisteredModules(Compilation compilation, CancellationToken cancellationToken)
    {
        INamedTypeSymbol? registrationAttribute = compilation.GetTypeByMetadataName(ModuleRegistrationAttributeName);
        INamedTypeSymbol? moduleInterface = compilation.GetTypeByMetadataName(ModuleInterfaceName);
        if (registrationAttribute is null || moduleInterface is null)
        {
            return EquatableArray<ModuleInfo>.Empty;
        }

        ImmutableArray<ModuleInfo>.Builder builder = ImmutableArray.CreateBuilder<ModuleInfo>();

        CollectFromAssembly(compilation.Assembly, registrationAttribute, moduleInterface, builder);

        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CollectFromAssembly(reference, registrationAttribute, moduleInterface, builder);
        }

        return builder.ToImmutable();
    }

    private static void CollectFromAssembly(IAssemblySymbol assembly, INamedTypeSymbol registrationAttribute, INamedTypeSymbol moduleInterface, ImmutableArray<ModuleInfo>.Builder modules)
    {
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass is not INamedTypeSymbol attributeClass)
            {
                continue;
            }
            if (!SymbolEqualityComparer.Default.Equals(attributeClass.OriginalDefinition, registrationAttribute))
            {
                continue;
            }
            if (attributeClass.TypeArguments.Length != 1 || attributeClass.TypeArguments[0] is not INamedTypeSymbol moduleType)
            {
                continue;
            }
            if (!ModuleSymbolAnalyser.IsInstantiableModule(moduleType, moduleInterface))
            {
                continue;
            }

            ModuleInfo moduleInfo = new(moduleType.Name, moduleType.ContainingNamespace.ToDisplayString());
            modules.Add(moduleInfo);
        }
    }

    private static string GenerateModuleEndpointExtensions(StubInfo stub, EquatableArray<ModuleInfo> modules)
    {
        return ModuleEndpointExtensionsEmitter.Emit(stub.Namespace, stub.ClassName, stub.MethodName, modules);
    }

    #endregion

    #region Nested Types

    private readonly record struct StubInfo(string Namespace, string ClassName, string MethodName, LocationInfo? Location);

    #endregion
}
