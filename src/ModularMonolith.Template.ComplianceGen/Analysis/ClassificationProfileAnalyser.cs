using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModularMonolith.Template.ComplianceGen.Models;
using ModularMonolith.Template.SourceGenerators.Shared.Analysis;
using ModularMonolith.Template.SourceGenerators.Shared.Collections;
using ModularMonolith.Template.SourceGenerators.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ModularMonolith.Template.ComplianceGen.Analysis;

internal static class ClassificationProfileAnalyser
{
    #region Field Declarations

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    #endregion

    #region Internal Method Declarations

    public static bool IsCandidateProfile(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration
            && classDeclaration.BaseList is not null
            && classDeclaration.BaseList.Types.Count > 0;
    }

    public static ClassificationProfileInfo? GetProfileInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol profileSymbol)
        {
            return null;
        }

        INamedTypeSymbol? baseType = profileSymbol.BaseType;
        if (baseType is null
            || baseType.OriginalDefinition.MetadataName != "ClassificationProfile`1"
            || baseType.OriginalDefinition.ContainingNamespace?.ToDisplayString() != "ModularMonolith.Template.SharedKernel.Compliance")
        {
            return null;
        }

        if (baseType.TypeArguments.Length != 1 || baseType.TypeArguments[0] is not INamedTypeSymbol modelType)
        {
            return null;
        }

        List<ClassifiedMemberInfo> members = [];

        foreach (InvocationExpressionSyntax invocation in classDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!IsClassifyInvocation(invocation, out ArgumentSyntax? selectorArgument, out ArgumentSyntax? classificationArgument)
                || selectorArgument is null
                || classificationArgument is null)
            {
                continue;
            }

            ClassifiedMemberInfo member = AnalyzeClassifyCall(context.SemanticModel, selectorArgument, classificationArgument, cancellationToken);
            members.Add(member);
        }

        if (members.Count == 0)
        {
            return null;
        }

        string @namespace = profileSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : profileSymbol.ContainingNamespace.ToDisplayString();

        string key = string.IsNullOrEmpty(@namespace)
            ? profileSymbol.Name
            : $"{@namespace}.{profileSymbol.Name}";

        return new ClassificationProfileInfo(
            Key: key,
            Namespace: @namespace,
            ProfileTypeName: profileSymbol.Name,
            ProfileTypeKeyword: classDeclaration.Keyword.ValueText,
            Accessibility: SymbolFormatting.GetAccessibilityKeyword(profileSymbol.DeclaredAccessibility),
            ModelTypeName: modelType.ToDisplayString(FullyQualifiedFormat),
            Members: new EquatableArray<ClassifiedMemberInfo>([.. members]));
    }

    #endregion

    #region Private Method Declarations

    private static bool IsClassifyInvocation(InvocationExpressionSyntax invocation, out ArgumentSyntax? selectorArgument, out ArgumentSyntax? classificationArgument)
    {
        selectorArgument = null;
        classificationArgument = null;

        string? methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            _ => null,
        };

        if (methodName != "Classify" || invocation.ArgumentList.Arguments.Count != 2)
        {
            return false;
        }

        selectorArgument = invocation.ArgumentList.Arguments[0];
        classificationArgument = invocation.ArgumentList.Arguments[1];
        return true;
    }

    private static ClassifiedMemberInfo AnalyzeClassifyCall(SemanticModel semanticModel, ArgumentSyntax selectorArgument, ArgumentSyntax classificationArgument, CancellationToken cancellationToken)
    {
        LocationInfo? location = LocationInfo.CreateFrom(selectorArgument.Expression);
        string classificationName = ResolveClassificationName(semanticModel, classificationArgument.Expression, cancellationToken);

        ExpressionSyntax? lambdaBody = selectorArgument.Expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple.ExpressionBody,
            ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ExpressionBody,
            _ => null,
        };

        if (lambdaBody is not MemberAccessExpressionSyntax memberAccess
            || !TryBuildMemberPath(memberAccess, out string memberPath))
        {
            return new ClassifiedMemberInfo(
                MemberPath: string.Empty,
                ClassificationName: classificationName,
                IsString: false,
                IsSupportedSelector: false,
                MemberTypeName: string.Empty,
                Location: location);
        }

        ITypeSymbol? memberType = semanticModel.GetTypeInfo(memberAccess, cancellationToken).Type;
        bool isString = memberType?.SpecialType == SpecialType.System_String;
        string memberTypeName = memberType?.ToDisplayString(FullyQualifiedFormat) ?? "unknown";

        return new ClassifiedMemberInfo(
            MemberPath: memberPath,
            ClassificationName: classificationName,
            IsString: isString,
            IsSupportedSelector: true,
            MemberTypeName: memberTypeName,
            Location: location);
    }

    private static bool TryBuildMemberPath(MemberAccessExpressionSyntax memberAccess, out string memberPath)
    {
        Stack<string> segments = new();
        ExpressionSyntax current = memberAccess;

        while (current is MemberAccessExpressionSyntax access)
        {
            segments.Push(access.Name.Identifier.ValueText);
            current = access.Expression;
        }

        if (current is not IdentifierNameSyntax)
        {
            memberPath = string.Empty;
            return false;
        }

        memberPath = string.Join(".", segments);
        return true;
    }

    private static string ResolveClassificationName(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        ISymbol? symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
        if (symbol is IFieldSymbol field && field.ContainingType.TypeKind == TypeKind.Enum)
        {
            return $"{field.ContainingType.ToDisplayString(FullyQualifiedFormat)}.{field.Name}";
        }

        return expression.ToString();
    }

    #endregion
}
