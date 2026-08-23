using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC001 - Detects access to .Value property on Result&lt;TValue&gt; types without
/// a prior IsSuccess check or TryGetValue call in the nearest enclosing if statement,
/// ternary expression, or logical OR/AND short-circuited check.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultValueUnsafeAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC001";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Unsafe access to Result.Value",
        messageFormat: "Accessing 'Value' may throw if the Result is a failure. Use TryGetValue or check IsSuccess first.",
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
            ResultTypeRecognizer recognizer = ResultTypeRecognizer.Resolve(
                compilationContext.Compilation
            );
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeMemberAccess(nodeContext, recognizer),
                SyntaxKind.SimpleMemberAccessExpression
            );
        });
    }

    private static void AnalyzeMemberAccess(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.ValueText != "Value")
        {
            return;
        }

        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(
            memberAccess.Expression,
            context.CancellationToken
        );
        if (typeInfo.Type is not INamedTypeSymbol namedType)
        {
            return;
        }

        // Note: the previous local check required IsGenericType (i.e. specifically Result<T>,
        // not the non-generic Result). ResultTypeRecognizer.IsResultType matches both. This is
        // a deliberate widening, not an oversight — non-generic Result has no .Value member, so
        // a syntactic `.Value` access on a non-generic Result receiver only occurs in code that
        // already fails to compile for an unrelated reason (no such member). This analyzer
        // firing in that case adds a redundant diagnostic on already-broken code, not a false
        // positive on valid code.
        if (!recognizer.IsResultType(namedType))
        {
            return;
        }

        // Suppress: access is inside a conversion operator (by design, these operators are intentionally unsafe).
        if (IsInsideConversionOperator(memberAccess))
        {
            return;
        }

        // Suppress: null-forgiving operator (!) applied to the Value access indicates the developer
        // has already acknowledged the risk and suppressed nullable analysis explicitly.
        if (HasNullForgivingOperator(memberAccess))
        {
            return;
        }

        if (
            ResultAccessSafetyRecognizer.IsGuardedByIsSuccess(memberAccess, context.SemanticModel)
            || ResultAccessSafetyRecognizer.IsGuardedByTryCall(
                memberAccess,
                context.SemanticModel,
                "TryGetValue"
            )
            || ResultAccessSafetyRecognizer.IsGuardedByPrecedingExit(
                memberAccess,
                context.SemanticModel,
                triggerProperty: "IsFailure"
            )
        )
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            namedType.Name
        );
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInsideConversionOperator(MemberAccessExpressionSyntax memberAccess)
    {
        SyntaxNode? current = memberAccess.Parent;
        while (current != null)
        {
            if (current is ConversionOperatorDeclarationSyntax)
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

    private static bool HasNullForgivingOperator(MemberAccessExpressionSyntax memberAccess)
    {
        return memberAccess.Parent is PostfixUnaryExpressionSyntax postfix
            && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression);
    }
}
