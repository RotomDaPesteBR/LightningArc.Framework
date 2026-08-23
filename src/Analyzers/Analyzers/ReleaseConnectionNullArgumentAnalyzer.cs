using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC043 - Detects calls to ReleaseConnection(null) with a null literal argument.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReleaseConnectionNullArgumentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC043";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ReleaseConnection called with null argument",
        messageFormat: "ReleaseConnection called with null argument. This call has no effect.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
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
            RepositoryTypeRecognizer recognizer = RepositoryTypeRecognizer.Resolve(
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
        RepositoryTypeRecognizer recognizer
    )
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(
            invocation,
            context.CancellationToken
        );
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.Name != "ReleaseConnection")
        {
            return;
        }

        // Verify this is actually RepositoryBase's ReleaseConnection, not an unrelated method
        // of the same name on some other type — a bare name match here would false-positive on
        // any consumer-defined "ReleaseConnection(SomeConnection?)" method elsewhere.
        //
        // Note: ReleaseConnection is declared directly on RepositoryBase (protected, not
        // overridden by derived repositories), so methodSymbol.ContainingType resolves to
        // RepositoryBase itself here — not a subclass of it. InheritsFromRepositoryBase alone
        // walks only ancestors and would incorrectly return false for this exact case, so this
        // checks equality-with-RepositoryBase first, falling back to the inheritance walk in
        // case a future version of the library moves/overrides the method in a derived type.
        INamedTypeSymbol containingType = methodSymbol.ContainingType;
        bool isRepositoryReleaseConnection =
            recognizer.RepositoryBaseType != null
            && (
                SymbolEqualityComparer.Default.Equals(containingType, recognizer.RepositoryBaseType)
                || recognizer.InheritsFromRepositoryBase(containingType)
            );

        if (!isRepositoryReleaseConnection)
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
            case LiteralExpressionSyntax literal
                when literal.IsKind(SyntaxKind.NullLiteralExpression):
            // Check for default (which would be null for reference types)
            case LiteralExpressionSyntax literalDefault
                when literalDefault.IsKind(SyntaxKind.DefaultLiteralExpression):
                return true;
            default:
                // Check for nameof(null) - unlikely but cover it
                return expression
                    is InvocationExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.ValueText: "default" },
                        ArgumentList.Arguments.Count: 0
                    };
        }
    }
}
