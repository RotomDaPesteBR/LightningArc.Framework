using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC008 - Detects <c>error += ...</c> accumulation inside a loop. Each <c>+=</c> combination
/// re-flattens the accumulated <c>AggregateError</c> from scratch (see
/// <c>Error.operator+</c>), making a loop of N accumulations roughly O(N²) work. A single-pass
/// batch combinator (<c>Error.Aggregate(...)</c>) or the <c>ResultAggregator</c> fluent API
/// (<c>Check</c>/<c>Ensure</c>/<c>When</c>/<c>CheckAll</c>/<c>WhenAll</c>) both do the same job
/// in one pass and are the idiomatic way this library expects validation accumulation to be
/// written.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ErrorAccumulationInLoopAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC008";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Error accumulated via '+=' inside a loop",
        messageFormat: "Accumulating an Error via '+=' inside a loop re-flattens on every iteration. Consider Error.Aggregate(...) or ResultAggregator's Check/Ensure/When methods instead.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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
                nodeContext => AnalyzeAssignment(nodeContext, recognizer),
                SyntaxKind.AddAssignmentExpression
            );
        });
    }

    private static void AnalyzeAssignment(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (!IsInsideLoop(assignment))
        {
            return;
        }

        TypeInfo leftTypeInfo = context.SemanticModel.GetTypeInfo(
            assignment.Left,
            context.CancellationToken
        );
        if (leftTypeInfo.Type == null || !recognizer.IsErrorType(leftTypeInfo.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation()));
    }

    /// <summary>
    /// Walks upward for the nearest loop construct (for/foreach/while/do), stopping at a
    /// method/lambda/local-function boundary — a <c>+=</c> inside a lambda defined *within* a
    /// loop body still counts (the lambda itself isn't the boundary that matters, the enclosing
    /// executable scope is), but a <c>+=</c> in a method that happens to be *called from* a loop
    /// elsewhere does not, since that's outside this analyzer's syntactic reach and would need
    /// call-graph analysis this rule intentionally doesn't attempt.
    /// </summary>
    private static bool IsInsideLoop(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;
        while (current != null)
        {
            if (
                current
                is ForStatementSyntax
                    or ForEachStatementSyntax
                    or ForEachVariableStatementSyntax
                    or WhileStatementSyntax
                    or DoStatementSyntax
            )
            {
                return true;
            }

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }
}
