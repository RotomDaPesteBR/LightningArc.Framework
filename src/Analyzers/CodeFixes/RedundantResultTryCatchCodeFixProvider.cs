using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC006 - removes a redundant try/catch that only maps exceptions to a
/// standardized Error/Result, splicing the try block's statements directly into the enclosing
/// scope in its place.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantResultTryCatchCodeFixProvider))]
[Shared]
public class RedundantResultTryCatchCodeFixProvider : CodeFixProvider
{
    private const string _title = "Remove redundant try-catch";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [RedundantResultTryCatchAnalyzer.DiagnosticId];

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

        // The diagnostic location is the try keyword itself (a token, not a node span) — its
        // Parent is directly the TryStatementSyntax, so FindToken().Parent is the precise tool
        // here, same reasoning as the reference examples' member-access lookups.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindToken(diagnosticSpan.Start).Parent;

        TryStatementSyntax? tryStatement =
            node as TryStatementSyntax ?? node?.FirstAncestorOrSelf<TryStatementSyntax>();

        if (tryStatement == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c => RemoveTryCatchAsync(context.Document, tryStatement, c),
                equivalenceKey: nameof(RedundantResultTryCatchCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> RemoveTryCatchAsync(
        Document document,
        TryStatementSyntax tryStatement,
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

        SyntaxList<StatementSyntax> innerStatements = tryStatement.Block.Statements;

        if (innerStatements.Count == 0)
        {
            SyntaxNode? emptyResultRoot = root.RemoveNode(
                tryStatement,
                SyntaxRemoveOptions.KeepNoTrivia
            );
            return emptyResultRoot == null ? document : document.WithSyntaxRoot(emptyResultRoot);
        }

        // Preserve the try statement's own leading trivia (e.g. a comment directly above it) on
        // the first spliced-in statement, so it isn't silently dropped. Tag with
        // Formatter.Annotation so re-indentation matches the new (un-nested) scope rather than
        // keeping the try block's original one-level-deeper indentation.
        StatementSyntax first = innerStatements[0]
            .WithLeadingTrivia(tryStatement.GetLeadingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);
        List<StatementSyntax> replacementStatements =
        [
            first,
            .. innerStatements
                .RemoveAt(0)
                .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation)),
        ];

        SyntaxNode newRoot = root.ReplaceNode(tryStatement, replacementStatements);
        return document.WithSyntaxRoot(newRoot);
    }
}
