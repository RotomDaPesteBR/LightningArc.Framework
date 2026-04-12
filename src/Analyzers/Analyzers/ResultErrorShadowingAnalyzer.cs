using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC015 - Detects when a new error is returned without consuming the original error
/// in a failure path, leading to potential loss of traceability.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultErrorShadowingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC015";
    public const string HelpLinkBase = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result error is shadowed",
        messageFormat: "A new error is being returned without consuming the original error '{0}'. Traceability may be lost.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeAction(AnalyzeArrowExpression, SyntaxKind.ArrowExpressionClause);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;
        if (returnStatement.Expression == null) return;

        AnalyzeExpression(context, returnStatement.Expression);
    }

    private static void AnalyzeArrowExpression(SyntaxNodeAnalysisContext context)
    {
        var arrowExpression = (ArrowExpressionClauseSyntax)context.Node;
        AnalyzeExpression(context, arrowExpression.Expression);
    }

    private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        // 1. Is the returned expression a new Error or Result.Failure?
        if (!IsNewErrorOrFailure(expression, context.SemanticModel)) return;

        // 2. Find any Result/Error variables in scope that are in a failure state at this point
        var failureGuards = FindActiveFailureGuards(expression, context.SemanticModel);
        if (failureGuards.Length == 0) return;

        foreach (var guard in failureGuards)
        {
            // 3. Was the error from this guard consumed?
            if (!IsErrorConsumed(expression, guard, context.SemanticModel))
            {
                var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), guard.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsNewErrorOrFailure(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(expression);
        if (typeInfo.Type == null) return false;

        // If it's an Error type or a Result type
        bool isError = IsErrorType(typeInfo.Type);
        bool isResult = IsResultType(typeInfo.Type);

        if (!isError && !isResult) return false;

        // Creating a new instance is always a "new error"
        if (expression is ObjectCreationExpressionSyntax) return true;

        if (expression is InvocationExpressionSyntax invocation)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null) return false;

            // Any method returning an Error is considered a "new error" creation/factory
            if (IsErrorType(symbol.ReturnType)) return true;

            // Result.Failure(...)
            if (symbol.Name == "Failure" && IsResultType(symbol.ContainingType)) return true;
        }

        return false;
    }

    private static ImmutableArray<ISymbol> FindActiveFailureGuards(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var results = ImmutableArray.CreateBuilder<ISymbol>();
        var current = expression.Parent;

        while (current != null)
        {
            if (current is IfStatementSyntax ifStmt)
            {
                // We are looking for guards that prove a variable is in a failure state
                var failureSymbol = GetGuardedFailureSymbol(ifStmt.Condition, semanticModel);
                if (failureSymbol != null)
                {
                    results.Add(failureSymbol);
                }
            }
            // Add more guard types if needed (switch, etc.)

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax) break;
            current = current.Parent;
        }

        return results.ToImmutable();
    }

    private static ISymbol? GetGuardedFailureSymbol(ExpressionSyntax condition, SemanticModel semanticModel)
    {
        // result.IsFailure
        if (condition is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.ValueText == "IsFailure")
        {
            return semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        }

        // !result.IsSuccess
        if (condition is PrefixUnaryExpressionSyntax prefix && prefix.IsKind(SyntaxKind.LogicalNotExpression) &&
            prefix.Operand is MemberAccessExpressionSyntax negated && negated.Name.Identifier.ValueText == "IsSuccess")
        {
            return semanticModel.GetSymbolInfo(negated.Expression).Symbol;
        }

        return null;
    }

    private static bool IsErrorConsumed(ExpressionSyntax returnExpression, ISymbol guardSymbol, SemanticModel semanticModel)
    {
        // Find the scope where the error could have been consumed
        // Usually the block containing the return statement
        var scope = returnExpression.Ancestors().OfType<BlockSyntax>().FirstOrDefault();
        if (scope == null) return false;

        // Look for any access to guardSymbol.Error or guardSymbol (if it's an Error type)
        var descendantNodes = scope.DescendantNodes().ToList();
        foreach (var node in descendantNodes)
        {
            if (node == returnExpression) continue; // Don't check the return itself yet (shadowing)

            if (node is MemberAccessExpressionSyntax ma && ma.Name.Identifier.ValueText == "Error")
            {
                var symbol = semanticModel.GetSymbolInfo(ma.Expression).Symbol;
                if (SymbolEqualityComparer.Default.Equals(symbol, guardSymbol)) return true;
            }

            // If guardSymbol is already an Error type, any use of it counts
            if (IsErrorType(guardSymbol.GetSymbolType()))
            {
                if (node is IdentifierNameSyntax id && SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(id).Symbol, guardSymbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsErrorType(ITypeSymbol? type)
    {
        if (type == null) return false;
        return type.Name == "Error" || (type.BaseType != null && IsErrorType(type.BaseType));
    }

    private static bool IsResultType(ITypeSymbol? type)
    {
        if (type == null) return false;
        if (type.Name == "Result") return true;
        return type.AllInterfaces.Any(i => i.Name.StartsWith("IResult"));
    }
}

internal static class SymbolExtensions
{
    public static ITypeSymbol? GetSymbolType(this ISymbol symbol)
    {
        return symbol switch
        {
            ILocalSymbol local => local.Type,
            IParameterSymbol param => param.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol prop => prop.Type,
            _ => null
        };
    }
}
