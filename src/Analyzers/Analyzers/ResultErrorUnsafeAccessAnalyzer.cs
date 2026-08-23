using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC002 - Detects access to .Error property on Result types without
/// a prior IsFailure check or TryGetError call in the nearest enclosing if statement,
/// ternary expression, or logical OR/AND short-circuited check.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultErrorUnsafeAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC002";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Unsafe access to Result.Error",
        messageFormat: "Accessing 'Error' may throw if the Result is a success. Use TryGetError or check IsFailure first.",
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

        if (memberAccess.Name.Identifier.ValueText != "Error")
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

        if (!recognizer.IsResultType(namedType))
        {
            return;
        }

        // Suppress: access is inside a conversion operator (by design, these operators are intentionally unsafe).
        if (IsInsideConversionOperator(memberAccess))
        {
            return;
        }

        // Suppress: null-forgiving operator (!) applied to the Error access indicates the developer
        // has already acknowledged the risk and suppressed nullable analysis explicitly.
        if (HasNullForgivingOperator(memberAccess))
        {
            return;
        }

        if (
            ResultAccessSafetyRecognizer.IsGuardedByIsFailure(memberAccess, context.SemanticModel)
            || ResultAccessSafetyRecognizer.IsGuardedByTryCall(
                memberAccess,
                context.SemanticModel,
                "TryGetError"
            )
            || ResultAccessSafetyRecognizer.IsGuardedByPrecedingExit(
                memberAccess,
                context.SemanticModel,
                triggerProperty: "IsSuccess"
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
            switch (current)
            {
                case ConversionOperatorDeclarationSyntax:
                    return true;
                case MethodDeclarationSyntax or LocalFunctionStatementSyntax:
                    return false;
                default:
                    current = current.Parent;
                    break;
            }
        }

        return false;
    }

    private static bool HasNullForgivingOperator(MemberAccessExpressionSyntax memberAccess)
    {
        return memberAccess.Parent is PostfixUnaryExpressionSyntax postfix
            && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression);
    }
}
