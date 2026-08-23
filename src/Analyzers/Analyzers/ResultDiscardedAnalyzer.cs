using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC003 - Detects ExpressionStatement where the invocation returns a Result type
/// without consuming it.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultDiscardedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC003";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result value is discarded",
        messageFormat: "Result return value is discarded. Potential errors may be silently ignored.",
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
                nodeContext => AnalyzeExpressionStatement(nodeContext, recognizer),
                SyntaxKind.ExpressionStatement
            );
        });
    }

    private static void AnalyzeExpressionStatement(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        if (context.Node is not ExpressionStatementSyntax expressionStatement)
        {
            return;
        }

        if (expressionStatement.Expression is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(
            invocation,
            context.CancellationToken
        );
        if (typeInfo.Type is not INamedTypeSymbol returnType)
        {
            return;
        }

        if (!recognizer.IsResultType(returnType))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
