using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightningArc.Analyzers;

/// <summary>
/// Shared helper for detecting when a Result.Value or Result.Error access is guarded
/// by a prior IsSuccess/IsFailure check or TryGetValue/TryGetError call.
///
/// Supports three short-circuit forms:
///  1. if-statement: if (result.IsFailure) { ... result.Value ... } — Value in the "else"
///     branch is guarded when the "if" checks IsFailure.
///     if (!result.IsSuccess) { return ... } // result.Value here is safe.
///  2. Ternary: result.IsSuccess ? result.Value : ... — Value in the whenTrue branch
///     is guarded; result.IsSuccess ? ... : result.Error — Error in whenFalse branch is guarded.
///     The negated forms (!result.IsSuccess / !result.IsFailure) swap branches.
///  3. Logical OR: result.IsFailure or result.Value — Value is guarded (LHS false -&gt; safe).
///     Logical AND: result.IsSuccess and result.Value — Value is guarded (LHS true -&gt; safe).
///     For LARC002: result.IsSuccess or result.Error — Error guarded (LHS true -&gt; safe).
///
/// Also tracks across chained || (OR) or &amp;&amp; (and) in the ternary condition.
/// </summary>
internal static class ResultGuardHelper
{
    /// <summary>
    /// Determines whether the given expression is guarded by an IsSuccess check.
    /// Walks up to nearest if-statement, ternary, or logical operator.
    /// </summary>
    public static bool IsGuardedByIsSuccess(
        MemberAccessExpressionSyntax access,
        SemanticModel semanticModel)
    {
        return IsGuarded(access, semanticModel, "IsSuccess", isTryCall: false);
    }

    /// <summary>
    /// Determines whether the given expression is guarded by an IsFailure check.
    /// Walks up to nearest if-statement, ternary, or logical operator.
    /// </summary>
    public static bool IsGuardedByIsFailure(
        MemberAccessExpressionSyntax access,
        SemanticModel semanticModel)
    {
        return IsGuarded(access, semanticModel, "IsFailure", isTryCall: false);
    }

    /// <summary>
    /// Determines whether the given expression is guarded by a TryXxx call.
    /// </summary>
    public static bool IsGuardedByTryCall(
        MemberAccessExpressionSyntax access,
        SemanticModel semanticModel,
        string tryMethodName)
    {
        return IsGuarded(access, semanticModel, tryMethodName, isTryCall: true);
    }

    /// <summary>
    /// Determines whether the given expression is guarded by a preceding early-return
    /// guard clause — e.g. <c>if (result.IsFailure) { ...; return; }</c> followed later
    /// in the same block by <c>result.Value</c>. This is a distinct topology from
    /// <see cref="IsGuardedByIsSuccess"/>/<see cref="IsGuardedByIsFailure"/>: those walk
    /// *upward* from the access looking for a wrapping guard (if/ternary/binary); this
    /// walks *backward* through preceding sibling statements in the same block, since the
    /// guard clause here doesn't wrap the access at all — it exits before the access is
    /// ever reached, which is exactly what makes the access safe.
    /// </summary>
    /// <param name="access">The member access being checked (e.g. <c>result.Value</c>).</param>
    /// <param name="semanticModel">The semantic model for symbol resolution.</param>
    /// <param name="triggerProperty">
    /// The property whose truthiness, if it caused a preceding unconditional exit, rules out
    /// that state for all code after the guard. For guarding <c>.Value</c> (LARC001), pass
    /// <c>"IsFailure"</c> — a preceding <c>if (result.IsFailure) { exit }</c> guarantees
    /// success afterward. For guarding <c>.Error</c> (LARC002), pass <c>"IsSuccess"</c>.
    /// </param>
    public static bool IsGuardedByPrecedingExit(
        MemberAccessExpressionSyntax access,
        SemanticModel semanticModel,
        string triggerProperty)
    {
        StatementSyntax? containingStatement = access.FirstAncestorOrSelf<StatementSyntax>();

        // Only the access's own direct block is considered — this deliberately does not
        // walk further outward through nested block levels (e.g. a guard clause in an outer
        // block with the access inside a nested try/using block). That's a real but rarer
        // pattern; scoping to the direct block keeps this addition narrow and predictable
        // rather than risking a false "safe" suppression on a genuinely unsafe access.
        if (containingStatement?.Parent is not BlockSyntax block)
        {
            return false;
        }

        int index = block.Statements.IndexOf(containingStatement);
        if (index <= 0)
        {
            return false;
        }

        for (int i = index - 1; i >= 0; i--)
        {
            if (block.Statements[i] is IfStatementSyntax { Else: null } ifStmt
                && UnconditionallyExits(ifStmt.Statement)
                && PropertyCheck(ifStmt.Condition, access.Expression, semanticModel, triggerProperty, isTryCall: false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a statement unconditionally transfers control out of the current
    /// flow (return/throw/continue/break), directly or as the last statement of a block.
    /// Intentionally conservative: does not attempt general reachability analysis (e.g. an
    /// exhaustive switch, or an if/else where every branch exits) — only the common single
    /// guard-clause shapes actually seen in this codebase's early-return pattern.
    /// </summary>
    private static bool UnconditionallyExits(StatementSyntax statement)
    {
        return statement switch
        {
            ReturnStatementSyntax or ThrowStatementSyntax or ContinueStatementSyntax or BreakStatementSyntax => true,
            BlockSyntax block when block.Statements.Count > 0 => UnconditionallyExits(block.Statements[^1]),
            _ => false
        };
    }

    private static bool IsGuarded(
        MemberAccessExpressionSyntax access,
        SemanticModel semanticModel,
        string checkProperty,
        bool isTryCall)
    {
        SyntaxNode? current = access.Parent;
        while (current != null)
        {
            switch (current)
            {
                case IfStatementSyntax ifStmt:
                {
                    // Access in the else-block means the condition check is negated
                    // (e.g., if (IsSuccess) { } else { result.Value } — IsSuccess guards Value here).
                    bool inElse = IsInElseBlock(ifStmt, access);
                    string effectiveProperty = inElse
                        ? (checkProperty == "IsSuccess" ? "IsFailure" : "IsSuccess")
                        : checkProperty;

                    if (ConditionChecks(ifStmt.Condition, access.Expression, semanticModel, effectiveProperty, isTryCall))
                    {
                        return true;
                    }

                    break;
                }

                case ConditionalExpressionSyntax conditional:
                {
                    if (conditional.WhenTrue == access || IsDescendantOf(access, conditional.WhenTrue))
                    {
                        // Expression is in the "true" branch — guard must check the property
                        if (ConditionChecks(conditional.Condition, access.Expression, semanticModel, checkProperty, isTryCall))
                        {
                            return true;
                        }
                    }
                    else if (conditional.WhenFalse == access || IsDescendantOf(access, conditional.WhenFalse))
                    {
                        // Expression is in the "false" branch — guard must check the negate
                        string negated = checkProperty == "IsSuccess" ? "IsFailure" : "IsSuccess";
                        if (ConditionChecks(conditional.Condition, access.Expression, semanticModel, negated, isTryCall))
                        {
                            return true;
                        }
                    }
                    break;
                }

                case BinaryExpressionSyntax binary:
                {
                    if (binary.Kind() == SyntaxKind.LogicalOrExpression ||
                        binary.Kind() == SyntaxKind.LogicalAndExpression)
                    {
                        // Expression in RHS — check if LHS guards it.
                        // For ||: if LHS is true, RHS is skipped -> RHS is safe.
                        //   LARC001 (IsSuccess): result.IsFailure || result.Value  — IsFailure on LHS guards Value.
                        //   LARC002 (IsFailure): result.IsSuccess || result.Error   — IsSuccess on LHS guards Error.
                        // For &&: if LHS is false, RHS is skipped -> RHS is safe.
                        //   LARC001 (IsSuccess): result.IsSuccess && result.Value   — IsSuccess on LHS guards Value.
                        //   LARC002 (IsFailure): result.IsFailure && result.Error   — IsFailure on LHS guards Error.
                        if (binary.Right == access || IsDescendantOf(access, binary.Right))
                        {
                            // For ||, the guard property is the opposite (LHS false -> safe).
                            // For &&, the guard property is the same (LHS true -> safe).
                            string guardProperty = binary.Kind() == SyntaxKind.LogicalOrExpression
                                ? (checkProperty == "IsSuccess" ? "IsFailure" : "IsSuccess")
                                : checkProperty;

                            if (ConditionChecks(binary.Left, access.Expression, semanticModel, guardProperty, isTryCall))
                            {
                                return true;
                            }
                        }
                    }
                    break;
                }
            }

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
            {
                break;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool ConditionChecks(
        ExpressionSyntax condition,
        ExpressionSyntax resultExpression,
        SemanticModel semanticModel,
        string checkProperty,
        bool isTryCall)
    {
        if (isTryCall)
        {
            return TryCallCheck(condition, resultExpression, semanticModel, checkProperty);
        }

        return PropertyCheck(condition, resultExpression, semanticModel, checkProperty, isTryCall);
    }

    private static bool PropertyCheck(
        ExpressionSyntax condition,
        ExpressionSyntax resultExpression,
        SemanticModel semanticModel,
        string checkProperty,
        bool isTryCall)
    {
        switch (condition)
        {
            // Direct check: result.IsFailure / result.IsSuccess
            case MemberAccessExpressionSyntax member when member.Name.Identifier.ValueText == checkProperty:
                return ExpressionsAreEquivalent(member.Expression, resultExpression) ||
                       ExpressionReferencesSameResult(member.Expression, resultExpression, semanticModel);
            // Negated check: !result.IsSuccess / !result.IsFailure
            case PrefixUnaryExpressionSyntax prefix when
                prefix.OperatorToken.IsKind(SyntaxKind.ExclamationToken) &&
                prefix.Operand is MemberAccessExpressionSyntax negated:
            {
                string negatedProp = checkProperty == "IsSuccess" ? "IsFailure" : "IsSuccess";
                if (negated.Name.Identifier.ValueText == negatedProp)
                {
                    return ExpressionsAreEquivalent(negated.Expression, resultExpression) ||
                           ExpressionReferencesSameResult(negated.Expression, resultExpression, semanticModel);
                }

                break;
            }
            // Bare result identifier: result -> checks IsSuccess
            case IdentifierNameSyntax identifier when !isTryCall:
            {
                // Only treat bare identifier as IsSuccess check when we're looking for IsSuccess guarding
                if (checkProperty == "IsSuccess")
                {
                    return ExpressionReferencesSameResult(identifier, resultExpression, semanticModel);
                }
                break;
            }
            // Negated bare result identifier: !result -> checks IsFailure
            case PrefixUnaryExpressionSyntax prefix when
                prefix.OperatorToken.IsKind(SyntaxKind.ExclamationToken) &&
                prefix.Operand is IdentifierNameSyntax identifier &&
                !isTryCall:
            {
                // Treat !result as IsFailure check when we're looking for IsFailure guarding
                if (checkProperty == "IsFailure")
                {
                    return ExpressionReferencesSameResult(identifier, resultExpression, semanticModel);
                }
                break;
            }
        }

        return false;
    }

    private static bool TryCallCheck(
        ExpressionSyntax condition,
        ExpressionSyntax resultExpression,
        SemanticModel semanticModel,
        string tryMethodName)
    {
        if (condition is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax member)
        {
            return false;
        }

        if (member.Name.Identifier.ValueText != tryMethodName)
        {
            return false;
        }

        return ExpressionsAreEquivalent(member.Expression, resultExpression) ||
               ExpressionReferencesSameResult(member.Expression, resultExpression, semanticModel);
    }

    private static bool IsDescendantOf(SyntaxNode node, SyntaxNode? ancestor)
    {
        SyntaxNode? current = node.Parent;
        while (current != null)
        {
            if (current == ancestor)
            {
                return true;
            }

            current = current.Parent;
        }
        return false;
    }

    private static bool IsInElseBlock(IfStatementSyntax ifStmt, MemberAccessExpressionSyntax access)
    {
        if (ifStmt.Else == null)
        {
            return false;
        }

        StatementSyntax elseStatement = ifStmt.Else.Statement;
        SyntaxNode? current = access.Parent;
        while (current != null)
        {
            if (current == elseStatement)
            {
                return true;
            }

            if (current == ifStmt)
            {
                return false;
            }

            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
            {
                return false;
            }

            current = current.Parent;
        }
        return false;
    }

    internal static bool ExpressionsAreEquivalent(ExpressionSyntax a, ExpressionSyntax b)
    {
        return a.ToString().Trim() == b.ToString().Trim();
    }

    internal static bool ExpressionReferencesSameResult(
        ExpressionSyntax a,
        ExpressionSyntax b,
        SemanticModel semanticModel)
    {
        ISymbol? symbolA = GetSymbol(a, semanticModel);
        ISymbol? symbolB = GetSymbol(b, semanticModel);

        if (symbolA != null && symbolB != null)
        {
            return SymbolEqualityComparer.Default.Equals(symbolA, symbolB);
        }

        return false;
    }

    internal static ISymbol? GetSymbol(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        return expression switch
        {
            IdentifierNameSyntax id => semanticModel.GetSymbolInfo(id).Symbol,
            MemberAccessExpressionSyntax member => semanticModel.GetSymbolInfo(member.Expression).Symbol,
            _ => semanticModel.GetSymbolInfo(expression).Symbol
        };
    }
}
