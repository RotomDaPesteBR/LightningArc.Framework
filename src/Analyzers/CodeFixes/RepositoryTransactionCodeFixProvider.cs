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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC041 - adds the missing <c>transaction: Transaction</c> named argument to a
/// database operation call made from a <c>RepositoryBase</c>-derived type.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RepositoryTransactionCodeFixProvider))]
[Shared]
public class RepositoryTransactionCodeFixProvider : CodeFixProvider
{
    private const string _title = "Pass Transaction to this call";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [RepositoryTransactionAnalyzer.DiagnosticId];

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

        // Diagnostic location is the whole invocation's span here, same as LARC040 — FindNode
        // resolves directly to the InvocationExpressionSyntax.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        InvocationExpressionSyntax? invocation =
            node as InvocationExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    AddTransactionArgumentAsync(context.Document, invocation, c),
                equivalenceKey: nameof(RepositoryTransactionCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> AddTransactionArgumentAsync(
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

        ArgumentSyntax transactionArgument = Argument(IdentifierName("Transaction"))
            .WithNameColon(NameColon("transaction"));

        // Appended as a named argument — safe regardless of the target method's actual
        // parameter order, since named arguments don't need to match positional order.
        ArgumentListSyntax newArgumentList = invocation
            .ArgumentList.AddArguments(transactionArgument)
            .WithAdditionalAnnotations(Formatter.Annotation);
        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);

        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
