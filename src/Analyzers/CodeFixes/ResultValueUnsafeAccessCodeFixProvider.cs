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
/// Code fix for LARC001 - Wraps .Value access in a TryGetValue check.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultValueUnsafeAccessCodeFixProvider))]
[Shared]
public class ResultValueUnsafeAccessCodeFixProvider : CodeFixProvider
{
    private const string _title = "Use TryGetValue instead";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ResultValueUnsafeAccessAnalyzer.DiagnosticId];

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context
            .Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        // Try to find the member access at the diagnostic location
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindToken(diagnosticSpan.Start).Parent;

        // Walk up to find the MemberAccessExpressionSyntax containing ".Value"
        MemberAccessExpressionSyntax? memberAccess = node
            ?.AncestorsAndSelf()
            .OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault(ma =>
                ma.Name.Span.Start == diagnosticSpan.Start
                && ma.Name.Identifier.ValueText == "Value"
            );

        if (memberAccess == null)
        {
            return;
        }

        // Find the enclosing statement
        StatementSyntax? statement = memberAccess
            .AncestorsAndSelf()
            .OfType<StatementSyntax>()
            .FirstOrDefault();

        if (statement == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    ApplyFixAsync(context.Document, root, memberAccess, statement, c),
                equivalenceKey: nameof(ResultValueUnsafeAccessCodeFixProvider)
            ),
            diagnostics: context.Diagnostics
        );
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        SyntaxNode root,
        MemberAccessExpressionSyntax memberAccess,
        StatementSyntax statement,
        CancellationToken _
    )
    {
        ExpressionSyntax resultExpression = memberAccess.Expression;

        // Build: result.TryGetValue(out var value)
        InvocationExpressionSyntax tryGetValueInvocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    resultExpression.WithoutLeadingTrivia().WithoutTrailingTrivia(),
                    IdentifierName("TryGetValue")
                )
            )
            .WithArgumentList(
                ArgumentList(
                    SeparatedList([
                        Argument(
                                DeclarationExpression(
                                    IdentifierName("var"),
                                    SingleVariableDesignation(Identifier("value"))
                                )
                            )
                            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword)),
                    ])
                )
            );

        // Build the replacement block statement
        StatementSyntax rewritten = RewriteValueAccess(statement, memberAccess);

        IfStatementSyntax ifStatement = IfStatement(
                tryGetValueInvocation,
                Block(rewritten.WithLeadingTrivia(Space))
            )
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(
            statement,
            ifStatement.WithAdditionalAnnotations(Formatter.Annotation)
        );
        return document.WithSyntaxRoot(newRoot);
    }

    private static StatementSyntax RewriteValueAccess(
        StatementSyntax statement,
        MemberAccessExpressionSyntax valueAccess
    )
    {
        ValueAccessRewriter rewriter = new(valueAccess);
        return (StatementSyntax)rewriter.Visit(statement);
    }

    private class ValueAccessRewriter(MemberAccessExpressionSyntax valueAccess)
        : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitMemberAccessExpression(
            MemberAccessExpressionSyntax node
        ) =>
            node.Name.Identifier.ValueText == "Value"
            && node.Expression.IsEquivalentTo(valueAccess.Expression)
                ? IdentifierName("value")
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(node.GetTrailingTrivia())
                : base.VisitMemberAccessExpression(node);
    }
}
