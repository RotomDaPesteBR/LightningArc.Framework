using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC003 - inserts a discard ('_ = ') in front of a Result-returning
/// invocation whose return value is currently discarded implicitly.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultDiscardedCodeFixProvider))]
[Shared]
public class ResultDiscardedCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ResultDiscardedAnalyzer.DiagnosticId];

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

        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        ExpressionStatementSyntax? statement = node
            ?.AncestorsAndSelf()
            .OfType<ExpressionStatementSyntax>()
            .FirstOrDefault();

        if (statement == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: DiscardExpressionFixer.Title,
                createChangedDocument: c =>
                    DiscardExpressionFixer.ApplyAsync(context.Document, statement, c),
                equivalenceKey: nameof(ResultDiscardedCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }
}
