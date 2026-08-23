using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Shared fix logic for turning <c>SomeCall();</c> into <c>_ = SomeCall();</c> — the identical
/// mechanical fix used by both <see cref="ResultDiscardedCodeFixProvider"/> (LARC003) and
/// <see cref="ValueObjectCreationDiscardedCodeFixProvider"/> (LARC022), so the rewrite logic
/// lives in exactly one place rather than being duplicated across two providers whose only
/// difference is which diagnostic they respond to.
/// </summary>
internal static class DiscardExpressionFixer
{
    public const string Title = "Discard the return value with '_'";

    public static async Task<Document> ApplyAsync(
        Document document,
        ExpressionStatementSyntax statement,
        CancellationToken cancellationToken
    )
    {
        SyntaxNode? root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        AssignmentExpressionSyntax discardAssignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(Identifier("_")),
            statement.Expression.WithoutTrivia()
        );

        ExpressionStatementSyntax newStatement = statement
            .WithExpression(discardAssignment)
            .WithTriviaFrom(statement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(statement, newStatement);
        return document.WithSyntaxRoot(newRoot);
    }
}
