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

        // Suppress: access is inside a conversion operator (by design, these operators are intentionally unsafe).
        if (IsInsideConversionOperator(memberAccess))
            return;

        // Suppress: null-forgiving operator (!) applied to the Value access indicates the developer
        // has already acknowledged the risk and suppressed nullable analysis explicitly.
        if (HasNullForgivingOperator(memberAccess))
            return;

        if (ResultGuardHelper.IsGuardedByIsSuccess(memberAccess, context.SemanticModel) ||
            ResultGuardHelper.IsGuardedByTryCall(memberAccess, context.SemanticModel, "TryGetValue"))
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

    private static bool IsInsideConversionOperator(MemberAccessExpressionSyntax memberAccess)
    {
        var current = memberAccess.Parent;
        while (current != null)
        {
            if (current is ConversionOperatorDeclarationSyntax)
                return true;

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
                return false;

            current = current.Parent;
        }

        return false;
    }

    private static bool HasNullForgivingOperator(MemberAccessExpressionSyntax memberAccess)
    {
        return memberAccess.Parent is PostfixUnaryExpressionSyntax postfix &&
               postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression);
    }
}
