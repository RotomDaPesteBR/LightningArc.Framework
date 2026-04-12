using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC030 - Detects calls to ReleaseConnection(null) with a null literal argument.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReleaseConnectionNullArgumentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC030";
    public const string HelpLinkBase = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ReleaseConnection called with null argument",
        messageFormat: "ReleaseConnection called with null argument. This call has no effect.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.Name != "ReleaseConnection")
        {
            return;
        }

        ArgumentListSyntax? argList = invocation.ArgumentList;
        if (argList.Arguments.Count != 1)
        {
            return;
        }

        ArgumentSyntax arg = argList.Arguments[0];
        if (!IsNullOrNullable(arg.Expression))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsNullOrNullable(ExpressionSyntax expression)
    {
        switch (expression)
        {
            // Check for null literal
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
            // Check for default (which would be null for reference types)
            case LiteralExpressionSyntax literalDefault when literalDefault.IsKind(SyntaxKind.DefaultLiteralExpression):
                return true;
            default:
                // Check for nameof(null) - unlikely but cover it
                return expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "default" }, ArgumentList.Arguments.Count: 0 };
        }
    }
}
