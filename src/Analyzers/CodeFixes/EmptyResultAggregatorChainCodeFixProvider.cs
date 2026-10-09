using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
/// Code fix for LARC007 - replaces a check-less sync
/// <c>Result.Aggregate().Build()</c> chain with <c>Result.Success()</c>, which is
/// what the empty chain always evaluates to. Never offered for
/// <c>BuildAsync()</c>: the async chain cannot be proven empty from syntax alone
/// (no API path produces a check-less <c>Task&lt;ResultAggregator&gt;</c>), so
/// rewriting it would risk discarding asynchronous work.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(EmptyResultAggregatorChainCodeFixProvider)
)]
[Shared]
public class EmptyResultAggregatorChainCodeFixProvider : CodeFixProvider
{
    private const string _title = "Replace with Result.Success()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [EmptyResultAggregatorChainAnalyzer.DiagnosticId];

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

        // Diagnostic location is the Build()/BuildAsync() invocation's span.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        InvocationExpressionSyntax? invocation =
            node as InvocationExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

        if (
            invocation?.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name.Identifier.ValueText != "Build"
        )
        {
            // Anything else (notably BuildAsync) is deliberately left without a fix.
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    ReplaceWithSuccessAsync(context.Document, invocation, c),
                equivalenceKey: nameof(EmptyResultAggregatorChainCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> ReplaceWithSuccessAsync(
        Document document,
        InvocationExpressionSyntax invocation,
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

        InvocationExpressionSyntax successCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Result"),
                    IdentifierName("Success")
                )
            )
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(invocation, successCall);
        return document.WithSyntaxRoot(newRoot);
    }
}
