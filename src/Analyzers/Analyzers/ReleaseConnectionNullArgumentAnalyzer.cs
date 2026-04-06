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
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

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
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.Name != "ReleaseConnection")
            return;

        var argList = invocation.ArgumentList;
        if (argList == null || argList.Arguments.Count != 1)
            return;

        var arg = argList.Arguments[0];
        if (IsNullOrNullable(arg.Expression))
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullOrNullable(ExpressionSyntax expression)
    {
        // Check for null literal
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression))
            return true;

        // Check for default (which would be null for reference types)
        if (expression is LiteralExpressionSyntax literalDefault && literalDefault.IsKind(SyntaxKind.DefaultLiteralExpression))
            return true;

        // Check for nameof(null) - unlikely but cover it
        if (expression is InvocationExpressionSyntax invocation &&
            (invocation.Expression as IdentifierNameSyntax)?.Identifier.ValueText == "default" &&
            invocation.ArgumentList.Arguments.Count == 0)
            return true;

        return false;
    }
}
