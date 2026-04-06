using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC003 - Detects ExpressionStatement where the invocation returns a type
/// containing "Result" in its display string.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultDiscardedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC003";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result value is discarded",
        messageFormat: "Result return value is discarded. Potential errors may be silently ignored.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
    }

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax expressionStatement)
            return;

        if (expressionStatement.Expression is not InvocationExpressionSyntax invocation)
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(invocation, context.CancellationToken);
        if (typeInfo.Type is not INamedTypeSymbol returnType)
            return;

        if (!returnType.Name.Contains("Result", StringComparison.OrdinalIgnoreCase))
            return;

        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
