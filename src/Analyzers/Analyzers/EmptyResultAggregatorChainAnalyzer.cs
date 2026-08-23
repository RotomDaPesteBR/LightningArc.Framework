using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC007 - Detects a ResultAggregator chain that goes straight from
/// <c>Result.Aggregate()</c> to <c>Build()</c>/<c>BuildAsync()</c> with no intermediate
/// <c>Check</c>/<c>CheckEach</c>/<c>CheckAsync</c>/<c>CheckAll</c>/<c>Ensure</c>/<c>When</c>/
/// <c>WhenAsync</c>/<c>WhenAll</c> call. Such a chain always succeeds and is almost always
/// either dead code or a check that was accidentally removed/forgotten.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyResultAggregatorChainAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC007";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ResultAggregator built with no checks",
        messageFormat: "Result.Aggregate() is built with no Check/Ensure/When/CheckAll/WhenAll calls in the chain. This always succeeds — likely dead code or a removed check.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Method names that legitimately extend a ResultAggregator fluent chain (both the sync
    /// ResultAggregator-receiver and Task&lt;ResultAggregator&gt;-mirror overloads share these
    /// names, so a single name set covers both).
    /// </summary>
    private static readonly ImmutableHashSet<string> ChainMethodNames = ImmutableHashSet.Create(
        "Check",
        "CheckEach",
        "CheckAsync",
        "CheckAll",
        "Ensure",
        "When",
        "WhenAsync",
        "WhenAll"
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            ResultTypeRecognizer recognizer = ResultTypeRecognizer.Resolve(
                compilationContext.Compilation
            );
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, recognizer),
                SyntaxKind.InvocationExpression
            );
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.ValueText;
        if (methodName != "Build" && methodName != "BuildAsync")
        {
            return;
        }

        var methodSymbol =
            context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol
            as IMethodSymbol;
        if (methodSymbol == null || !IsAggregatorBuildMethod(methodSymbol, recognizer))
        {
            return;
        }

        // Walk the receiver chain backward from Build()/BuildAsync(). If every intermediate
        // step is a recognized chain method, keep walking; the moment we either reach
        // Result.Aggregate() (report if nothing was seen along the way) or hit something we
        // don't recognize (bail out — can't confidently reason about it), stop.
        bool sawAnyCheck = false;
        ExpressionSyntax current = memberAccess.Expression;

        while (
            current is InvocationExpressionSyntax stepInvocation
            && stepInvocation.Expression is MemberAccessExpressionSyntax stepMember
        )
        {
            string stepName = stepMember.Name.Identifier.ValueText;

            if (stepName == "Aggregate")
            {
                var stepSymbol =
                    context
                        .SemanticModel.GetSymbolInfo(stepInvocation, context.CancellationToken)
                        .Symbol as IMethodSymbol;

                if (stepSymbol != null && recognizer.IsResultType(stepSymbol.ContainingType))
                {
                    if (!sawAnyCheck)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, invocation.GetLocation())
                        );
                    }
                }

                // Either reported (empty chain) or this "Aggregate" wasn't actually
                // Result.Aggregate() — either way, we've reached the root, nothing more to walk.
                return;
            }

            if (ChainMethodNames.Contains(stepName))
            {
                sawAnyCheck = true;
            }
            else
            {
                // Unrecognized method in the chain — could be an unrelated extension method
                // that happens to be chained here. Don't guess; only flag chains we can fully
                // account for end to end.
                return;
            }

            current = stepMember.Expression;
        }

        // Chain didn't resolve back to a direct Result.Aggregate() call (e.g. Build() called on
        // a variable holding a previously-built aggregator). Deliberately not flagged — proving
        // emptiness across a variable boundary would need dataflow analysis this rule doesn't do.
    }

    private static bool IsAggregatorBuildMethod(
        IMethodSymbol methodSymbol,
        ResultTypeRecognizer recognizer
    )
    {
        if (methodSymbol.Name == "Build")
        {
            return recognizer.IsResultType(methodSymbol.ReturnType);
        }

        if (methodSymbol.Name == "BuildAsync")
        {
            return methodSymbol.ReturnType is INamedTypeSymbol { Name: "Task" } taskType
                && taskType.IsGenericType
                && taskType.TypeArguments.Length == 1
                && recognizer.IsResultType(taskType.TypeArguments[0]);
        }

        return false;
    }
}
