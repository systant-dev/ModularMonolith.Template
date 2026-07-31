using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModularMonolith.Template.ModularGen.Analysis;
using ModularMonolith.Template.ModularGen.Diagnostics;
using ModularMonolith.Template.SourceGen.Common.Collections;
using ModularMonolith.Template.ModularGen.Models;
using System.Collections.Immutable;
using System.Text;

namespace ModularMonolith.Template.ModularGen.Generators;

[Generator]
public sealed class ModuleGenerator : IIncrementalGenerator
{
    #region Field Declarations

    private const string ModuleRegistrationAttributeName = "ModularMonolith.Template.SharedKernel.Modular.ModuleRegistrationAttribute`1";
    private const string ModuleInterfaceName = "ModularMonolith.Template.SharedKernel.Modular.IModule";
    private const string ModuleRegistrationTargetAttributeName = "ModularMonolith.Template.SharedKernel.Modular.ModuleRegistrationTargetAttribute";

    private const string ModulesConfigurationSection = "Modules";
    private const string ModuleEnabledConfigurationKey = "Enabled";
    private const bool ModuleEnabledByDefault = true;

    #endregion

    #region Public Method Declarations

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StubInfo>> stubProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ModuleRegistrationTargetAttributeName,
                predicate: static (node, _) => IsPartialStubMethod(node),
                transform: static (attributeContext, cancellationToken) => GetStubInfo(attributeContext))
            .Where(static stub => stub is not null)
            .Select(static (stub, _) => stub!.Value)
            .Collect();

        IncrementalValueProvider<EquatableArray<ModuleInfo>> modulesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => GetRegisteredModules(compilation, cancellationToken))
            .WithTrackingName("GetModules");

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
                sourceContext.ReportDiagnostic(Diagnostic.Create(
                    ModularDiagnostics.MultipleRegistrationTargets,
                    orderedStubs[index].Location?.ToLocation(),
                    $"{stub.ClassName}.{stub.MethodName}"));
            }

            string source = GenerateModuleExtensions(stub, pair.Right);
            sourceContext.AddSource("ModuleServiceCollectionExtensions.g.cs", source);
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

            modules.Add(new ModuleInfo(moduleType.Name, moduleType.ContainingNamespace.ToDisplayString()));
        }
    }

    private static string GenerateModuleExtensions(StubInfo stub, EquatableArray<ModuleInfo> modules)
    {
        StringBuilder builder = new();

        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using Microsoft.Extensions.Configuration;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using ModularMonolith.Template.SharedKernel.Modular;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(stub.Namespace);
        builder.AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("[global::System.CodeDom.Compiler.GeneratedCode(\"ModularMonolith.Template.ModularGen\", null)]");
        builder.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        builder.Append("public static partial class ");
        builder.AppendLine(stub.ClassName);
        builder.AppendLine("{");
        builder.Append("    private static partial void ");
        builder.Append(stub.MethodName);
        builder.AppendLine("(IServiceCollection serviceCollection, IConfiguration configuration)");
        builder.AppendLine("    {");

        int count = 0;
        foreach (ModuleInfo moduleInfo in modules.OrderBy(module => module.ModuleNamespace).ThenBy(module => module.ModuleName))
        {
            builder.Append("        if (configuration.GetValue<bool>(\"");
            builder.Append(ModulesConfigurationSection);
            builder.Append(':');
            builder.Append(moduleInfo.ModuleNamespace);
            builder.Append(':');
            builder.Append(ModuleEnabledConfigurationKey);
            builder.Append("\", ");
            builder.Append(ModuleEnabledByDefault ? "true" : "false");
            builder.AppendLine("))");
            builder.AppendLine("        {");
            builder.Append("            global::");
            builder.Append(moduleInfo.ModuleNamespace);
            builder.Append('.');
            builder.Append(moduleInfo.ModuleName);
            builder.AppendLine($" module{count} = new();");
            builder.AppendLine($"            module{count++}.RegisterServices(serviceCollection);");
            builder.AppendLine("        }");
            builder.AppendLine();
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    #endregion

    #region Nested Types

    private readonly record struct StubInfo(string Namespace, string ClassName, string MethodName, LocationInfo? Location);

    #endregion
}