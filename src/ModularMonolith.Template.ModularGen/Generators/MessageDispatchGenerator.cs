using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModularMonolith.Template.ModularGen.Emit;
using ModularMonolith.Template.ModularGen.Models;
using ModularMonolith.Template.SourceGenerators.Shared.Collections;
using ModularMonolith.Template.SourceGenerators.Shared.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.ModularGen.Generators;

[Generator]
public sealed class MessageDispatchGenerator : IIncrementalGenerator
{
    #region Field Declarations

    private const string DispatchMapTargetAttributeName = "ModularMonolith.Template.SharedKernel.Messaging.MessageDispatchMapTargetAttribute";

    private const string MessageHandlerInterfaceName = "ModularMonolith.Template.SharedKernel.Messaging.IMessageHandler`1";

    #endregion

    #region Public Method Declarations

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StubInfo>> stubProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DispatchMapTargetAttributeName,
                predicate: static (node, _) => IsPartialStubMethod(node),
                transform: static (attributeContext, _) => GetStubInfo(attributeContext))
            .Where(static stub => stub is not null)
            .Select(static (stub, _) => stub!.Value)
            .Collect();

        IncrementalValueProvider<EquatableArray<MessageInfo>> messagesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => GetMessageTypes(compilation, cancellationToken))
            .WithTrackingName("GetMessages");

        context.RegisterSourceOutput(stubProvider.Combine(messagesProvider), static (sourceContext, pair) =>
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

            string source = MessageDispatchEmitter.Emit(stub.Namespace, stub.ClassName, stub.MethodName, pair.Right);
            sourceContext.AddSource("MessageDispatchServiceCollectionExtensions.g.cs", source);
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
        StubInfo stubInfo = new(namespaceName, containingType.Name, methodSymbol.Name, location);
        return stubInfo;
    }

    private static EquatableArray<MessageInfo> GetMessageTypes(Compilation compilation, CancellationToken cancellationToken)
    {
        INamedTypeSymbol? handlerInterface = compilation.GetTypeByMetadataName(MessageHandlerInterfaceName);
        if (handlerInterface is null)
        {
            return EquatableArray<MessageInfo>.Empty;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        ImmutableArray<MessageInfo>.Builder builder = ImmutableArray.CreateBuilder<MessageInfo>();

        CollectFromNamespace(compilation.Assembly.GlobalNamespace, handlerInterface, seen, builder, cancellationToken);

        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CollectFromNamespace(reference.GlobalNamespace, handlerInterface, seen, builder, cancellationToken);
        }

        return builder.ToImmutable();
    }

    private static void CollectFromNamespace(INamespaceSymbol namespaceSymbol, INamedTypeSymbol handlerInterface, HashSet<string> seen, ImmutableArray<MessageInfo>.Builder messages, CancellationToken cancellationToken)
    {
        foreach (INamespaceOrTypeSymbol member in namespaceSymbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (member is INamespaceSymbol childNamespace)
            {
                CollectFromNamespace(childNamespace, handlerInterface, seen, messages, cancellationToken);
            }
            else if (member is INamedTypeSymbol type)
            {
                CollectFromType(type, handlerInterface, seen, messages, cancellationToken);
            }
        }
    }

    private static void CollectFromType(INamedTypeSymbol type, INamedTypeSymbol handlerInterface, HashSet<string> seen, ImmutableArray<MessageInfo>.Builder messages, CancellationToken cancellationToken)
    {
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            CollectFromType(nested, handlerInterface, seen, messages, cancellationToken);
        }

        if (type.IsAbstract || type.TypeKind != TypeKind.Class || type.IsUnboundGenericType || type.IsGenericType)
        {
            return;
        }

        foreach (INamedTypeSymbol implemented in type.AllInterfaces)
        {
            if (!SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, handlerInterface))
            {
                continue;
            }

            if (implemented.TypeArguments.Length != 1 || implemented.TypeArguments[0] is not INamedTypeSymbol messageType)
            {
                continue;
            }

            if (messageType.TypeKind == TypeKind.Error || messageType.IsUnboundGenericType)
            {
                continue;
            }

            string fullyQualifiedName = messageType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (seen.Add(fullyQualifiedName))
            {
                MessageInfo message = new(fullyQualifiedName);
                messages.Add(message);
            }
        }
    }

    #endregion

    #region Nested Types

    private readonly record struct StubInfo(string Namespace, string ClassName, string MethodName, LocationInfo? Location);

    #endregion
}
