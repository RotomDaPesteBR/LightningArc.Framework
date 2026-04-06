using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC001 - Detects access to .Value property on Result&lt;TValue&gt; types without
/// a prior IsSuccess check or TryGetValue call in the nearest enclosing if statement.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultValueUnsafeAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC001";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Unsafe access to Result.Value",
        messageFormat: "Accessing 'Value' may throw if the Result is a failure. Use TryGetValue or check IsSuccess first.",
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

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.ValueText != "Value")
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken);
        if (typeInfo.Type is not INamedTypeSymbol namedType)
            return;

        if (!IsResultType(namedType))
            return;

        if (IsGuarded(memberAccess, context.SemanticModel, context.CancellationToken))
            return;

        var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation(), namedType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsResultType(INamedTypeSymbol namedType)
    {
        if (namedType.IsGenericType && namedType.Name == "Result")
            return true;

        if (namedType.BaseType != null && IsResultType(namedType.BaseType))
            return true;

        foreach (var iface in namedType.AllInterfaces)
        {
            if (iface.Name.StartsWith("IResult"))
                return true;
        }

        return false;
    }

    private static bool IsGuardedByIsSuccess(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is IfStatementSyntax ifStatement)
            {
                if (ConditionChecksIsSuccess(ifStatement.Condition, memberAccess.Expression, semanticModel))
                    return true;
            }

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
                break;

            current = current.Parent;
        }

        return false;
    }

    private static bool IsGuardedByTryGetValue(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is IfStatementSyntax ifStatement)
            {
                if (ConditionCallsTryGetValue(ifStatement.Condition, memberAccess.Expression, semanticModel))
                    return true;
            }

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
                break;

            current = current.Parent;
        }

        return false;
    }

    private static bool ConditionChecksIsSuccess(
        ExpressionSyntax condition,
        ExpressionSyntax resultExpression,
        SemanticModel semanticModel)
    {
        // result.IsSuccess
        if (condition is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.ValueText == "IsSuccess")
        {
            return ExpressionsAreEquivalent(memberAccess.Expression, resultExpression) ||
                   ExpressionReferencesSameResult(memberAccess.Expression, resultExpression, semanticModel);
        }

        // !result.IsFailure (equivalent to IsSuccess)
        if (condition is PrefixUnaryExpressionSyntax prefix &&
            prefix.OperatorToken.IsKind(SyntaxKind.ExclamationToken) &&
            prefix.Operand is MemberAccessExpressionSyntax negatedAccess &&
            negatedAccess.Name.Identifier.ValueText == "IsFailure")
        {
            return ExpressionsAreEquivalent(negatedAccess.Expression, resultExpression) ||
                   ExpressionReferencesSameResult(negatedAccess.Expression, resultExpression, semanticModel);
        }

        return false;
    }

    private static bool ConditionCallsTryGetValue(
        ExpressionSyntax condition,
        ExpressionSyntax resultExpression,
        SemanticModel semanticModel)
    {
        if (condition is not InvocationExpressionSyntax invocation)
            return false;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name.Identifier.ValueText != "TryGetValue")
            return false;

        return ExpressionsAreEquivalent(memberAccess.Expression, resultExpression) ||
               ExpressionReferencesSameResult(memberAccess.Expression, resultExpression, semanticModel);
    }

    private static bool IsGuarded(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        return IsGuardedByIsSuccess(memberAccess, semanticModel, ct) ||
               IsGuardedByTryGetValue(memberAccess, semanticModel, ct);
    }

    private static bool ExpressionsAreEquivalent(ExpressionSyntax a, ExpressionSyntax b)
    {
        return a.ToString().Trim() == b.ToString().Trim();
    }

    private static bool ExpressionReferencesSameResult(
        ExpressionSyntax a,
        ExpressionSyntax b,
        SemanticModel semanticModel)
    {
        var symbolA = GetSymbol(a, semanticModel);
        var symbolB = GetSymbol(b, semanticModel);

        if (symbolA != null && symbolB != null)
            return SymbolEqualityComparer.Default.Equals(symbolA, symbolB);

        return false;
    }

    private static ISymbol? GetSymbol(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        return expression switch
        {
            IdentifierNameSyntax id => semanticModel.GetSymbolInfo(id).Symbol,
            MemberAccessExpressionSyntax member => semanticModel.GetSymbolInfo(member.Expression).Symbol,
            _ => semanticModel.GetSymbolInfo(expression).Symbol
        };
    }
}
