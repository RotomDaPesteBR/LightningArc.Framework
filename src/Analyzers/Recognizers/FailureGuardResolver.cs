using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightningArc.Analyzers;

/// <summary>
/// Shared resolution of "failure guard" variables: <c>Result</c>/<c>Error</c>
/// values proven to be in a failure state by an enclosing
/// <c>if (x.IsFailure)</c> / <c>if (!x.IsSuccess)</c> check. Used by both the
/// LARC005 analyzer and its code fix so the guard-derivation logic lives in
/// exactly one place.
/// </summary>
internal static class FailureGuardResolver
{
    /// <summary>
    /// Finds the symbols proven to be in a failure state at the point of
    /// <paramref name="expression"/>, by walking outward to the enclosing
    /// method/lambda/local-function boundary and collecting one symbol per
    /// enclosing failure-guard <c>if</c>.
    /// </summary>
    public static ImmutableArray<ISymbol> FindActiveFailureGuards(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        ResultTypeRecognizer recognizer
    )
    {
        var results = ImmutableArray.CreateBuilder<ISymbol>();
        var current = expression.Parent;

        while (current != null)
        {
            if (current is IfStatementSyntax ifStmt)
            {
                // We are looking for guards that prove a variable is in a failure state
                var failureSymbol = GetGuardedFailureSymbol(
                    ifStmt.Condition,
                    semanticModel,
                    recognizer
                );
                if (failureSymbol != null)
                {
                    results.Add(failureSymbol);
                }
            }
            // Add more guard types if needed (switch, etc.)

            if (
                current
                is MethodDeclarationSyntax
                    or LocalFunctionStatementSyntax
                    or AnonymousFunctionExpressionSyntax
            )
            {
                break;
            }

            current = current.Parent;
        }

        return results.ToImmutable();
    }

    private static ISymbol? GetGuardedFailureSymbol(
        ExpressionSyntax condition,
        SemanticModel semanticModel,
        ResultTypeRecognizer recognizer
    )
    {
        // result.IsFailure
        if (
            condition is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.ValueText == "IsFailure"
        )
        {
            return semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        }

        // !result.IsSuccess
        if (
            condition is PrefixUnaryExpressionSyntax prefix
            && prefix.IsKind(SyntaxKind.LogicalNotExpression)
            && prefix.Operand is MemberAccessExpressionSyntax negated
            && negated.Name.Identifier.ValueText == "IsSuccess"
        )
        {
            return semanticModel.GetSymbolInfo(negated.Expression).Symbol;
        }

        return null;
    }
}
