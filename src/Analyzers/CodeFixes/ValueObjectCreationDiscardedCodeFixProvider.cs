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
/// Code fix for LARC022 - inserts a discard ('_ = ') in front of a ValueObject
/// Create()/TryCreate() invocation whose return value is currently discarded implicitly.
/// </summary>
/// <remarks>
/// Identical fix shape to LARC003 (<see cref="ResultDiscardedCodeFixProvider"/>) — both delegate
/// to <see cref="DiscardExpressionFixer"/> rather than duplicating the rewrite logic.
/// </remarks>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(ValueObjectCreationDiscardedCodeFixProvider)
)]
[Shared]
public class ValueObjectCreationDiscardedCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ValueObjectCreationDiscardedAnalyzer.DiagnosticId];

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
                equivalenceKey: nameof(ValueObjectCreationDiscardedCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }
}
