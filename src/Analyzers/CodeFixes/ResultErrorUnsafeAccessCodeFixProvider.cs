using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC002 - Wraps .Error access in a TryGetError check.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultErrorUnsafeAccessCodeFixProvider))]
[Shared]
public class ResultErrorUnsafeAccessCodeFixProvider : CodeFixProvider
{
    private const string _title = "Use TryGetError instead";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ResultErrorUnsafeAccessAnalyzer.DiagnosticId];

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        // Try to find the member access at the diagnostic location
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindToken(diagnosticSpan.Start).Parent;

        // Walk up to find the MemberAccessExpressionSyntax containing ".Error"
        MemberAccessExpressionSyntax? memberAccess = node?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault(ma => ma.Name.Span.Start == diagnosticSpan.Start && ma.Name.Identifier.ValueText == "Error");

        if (memberAccess == null)
        {
            return;
        }

        // Find the enclosing statement
        StatementSyntax? statement = memberAccess.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
        if (statement == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c => ApplyFixAsync(context.Document, root, memberAccess, statement, c),
                equivalenceKey: nameof(ResultErrorUnsafeAccessCodeFixProvider)),
            diagnostics: context.Diagnostics);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        SyntaxNode root,
        MemberAccessExpressionSyntax memberAccess,
        StatementSyntax statement,
        CancellationToken _)
    {
        ExpressionSyntax resultExpression = memberAccess.Expression;

        // Build: result.TryGetError(out var err)
        InvocationExpressionSyntax tryGetErrorInvocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    resultExpression.WithoutLeadingTrivia().WithoutTrailingTrivia(),
                    IdentifierName("TryGetError")))
            .WithArgumentList(ArgumentList(SeparatedList([
                Argument(DeclarationExpression(
                    IdentifierName("var"),
                    SingleVariableDesignation(Identifier("err"))))
                    .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
            ])));

        // Build the replacement block statement
        StatementSyntax rewritten = RewriteErrorAccess(statement, memberAccess);

        IfStatementSyntax ifStatement = IfStatement(tryGetErrorInvocation,
                Block(rewritten.WithLeadingTrivia(Space)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(statement, ifStatement.WithAdditionalAnnotations(Formatter.Annotation));
        return document.WithSyntaxRoot(newRoot);
    }

    private static StatementSyntax RewriteErrorAccess(
        StatementSyntax statement,
        MemberAccessExpressionSyntax errorAccess)
    {
        ErrorAccessRewriter rewriter = new(errorAccess);
        return (StatementSyntax)rewriter.Visit(statement);
    }

    private class ErrorAccessRewriter(MemberAccessExpressionSyntax errorAccess) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Name.Identifier.ValueText == "Error" &&
                node.Expression.IsEquivalentTo(errorAccess.Expression))
            {
                return IdentifierName("err")
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(node.GetTrailingTrivia());
            }

            return base.VisitMemberAccessExpression(node);
        }
    }
}
