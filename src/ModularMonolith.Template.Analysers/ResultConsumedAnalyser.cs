using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace ModularMonolith.Template.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class ResultConsumedAnalyser : DiagnosticAnalyzer
{
    #region Field Declarations

    public const string DiagnosticId = "MM0003";

    private const string ResultTypeName = "ModularMonolith.Template.SharedKernel.Results.Result";
    private const string ResultOfTTypeName = "ModularMonolith.Template.SharedKernel.Results.Result`1";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result return value must be consumed",
        messageFormat: "The Result returned by '{0}' is discarded; assign it, return it, or inspect IsSuccess/Error",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A Result or Result<T> conveys the success or failure of an operation. Discarding it silently ignores failures, so the value must be observed.");

    #endregion

    #region Property Declarations

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    #endregion

    #region Public Method Declarations

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion

    #region Private Method Declarations

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        INamedTypeSymbol? resultType = context.Compilation.GetTypeByMetadataName(ResultTypeName);
        INamedTypeSymbol? resultOfTType = context.Compilation.GetTypeByMetadataName(ResultOfTTypeName);

        if (resultType is null && resultOfTType is null)
        {
            return;
        }

        context.RegisterOperationAction(
            operationContext => AnalyseExpressionStatement(operationContext, resultType, resultOfTType),
            OperationKind.ExpressionStatement);
    }

    private static void AnalyseExpressionStatement(OperationAnalysisContext context, INamedTypeSymbol? resultType, INamedTypeSymbol? resultOfTType)
    {
        IExpressionStatementOperation statement = (IExpressionStatementOperation)context.Operation;
        IOperation value = statement.Operation;

        // Only a bare method call (optionally awaited) discards the value. Assignments, discards
        // (_ = ...), returns, and argument usage are all legitimate consumption and are skipped.
        if (value is not (IInvocationOperation or IAwaitOperation))
        {
            return;
        }

        if (value.Type is not { } producedType || !IsResultType(producedType, resultType, resultOfTType))
        {
            return;
        }

        string producer = DescribeProducer(value);
        context.ReportDiagnostic(Diagnostic.Create(Rule, value.Syntax.GetLocation(), producer));
    }

    private static bool IsResultType(ITypeSymbol type, INamedTypeSymbol? resultType, INamedTypeSymbol? resultOfTType)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (resultType is not null && SymbolEqualityComparer.Default.Equals(current, resultType))
            {
                return true;
            }

            if (resultOfTType is not null &&
                current is INamedTypeSymbol { IsGenericType: true } named &&
                SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, resultOfTType))
            {
                return true;
            }
        }

        return false;
    }

    private static string DescribeProducer(IOperation operation)
    {
        IInvocationOperation? invocation = operation switch
        {
            IInvocationOperation direct => direct,
            IAwaitOperation await => await.Operation as IInvocationOperation,
            _ => null
        };

        return invocation?.TargetMethod.Name ?? "expression";
    }

    #endregion
}
