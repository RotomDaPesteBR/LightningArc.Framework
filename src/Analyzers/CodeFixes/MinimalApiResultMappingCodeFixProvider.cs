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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC060 - appends <c>.ToEndpointResult()</c> to the lambda's
/// returned expression. Expression-bodied lambdas need a single edit; block-bodied
/// lambdas get one edit per <c>return</c> (all applied by the same action).
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(MinimalApiResultMappingCodeFixProvider)
)]
[Shared]
public class MinimalApiResultMappingCodeFixProvider : CodeFixProvider
{
    private const string _title = "Append .ToEndpointResult()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [MinimalApiResultMappingAnalyzer.DiagnosticId];

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

        // Diagnostic location is the whole lambda's span.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        LambdaExpressionSyntax? lambda =
            node as LambdaExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<LambdaExpressionSyntax>().FirstOrDefault();

        if (lambda == null)
        {
            return;
        }

        if (lambda.ExpressionBody != null)
        {
            ExpressionSyntax body = lambda.ExpressionBody;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: _title,
                    createChangedDocument: c =>
                        AppendToExpressionsAsync(context.Document, [body], c),
                    equivalenceKey: nameof(MinimalApiResultMappingCodeFixProvider)
                ),
                context.Diagnostics[0]
            );

            return;
        }

        if (lambda.Block == null)
        {
            return;
        }

        // Only returns that belong to this lambda count — returns inside a
        // nested lambda, anonymous method, or local function declared in the
        // handler would map a different Result. The owner is the nearest
        // enclosing function: when it is not this exact lambda instance, the
        // return belongs to a nested function and is left untouched.
        // Ancestor comparison is by instance: these nodes all come from the
        // same root the lambda was located in, so the parent chain reaches
        // that exact lambda instance.
        List<ExpressionSyntax> returnedExpressions = lambda
            .Block.DescendantNodes()
            .OfType<ReturnStatementSyntax>()
            .Where(r =>
                r.Expression != null && OwningFunction(r) == (SyntaxNode)lambda
            )
            .Select(r => r.Expression!)
            .ToList();

        if (returnedExpressions.Count == 0)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    AppendToExpressionsAsync(context.Document, returnedExpressions, c),
                equivalenceKey: nameof(MinimalApiResultMappingCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static SyntaxNode? OwningFunction(ReturnStatementSyntax r) =>
        r.Ancestors().FirstOrDefault(a =>
            a
                is LambdaExpressionSyntax
                    or AnonymousMethodExpressionSyntax
                    or LocalFunctionStatementSyntax
                    or MethodDeclarationSyntax
        );

    private static InvocationExpressionSyntax ToEndpointResult(ExpressionSyntax expression) =>
        InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                expression.WithoutTrivia(),
                IdentifierName("ToEndpointResult")
            )
        );

    private static async Task<Document> AppendToExpressionsAsync(
        Document document,
        List<ExpressionSyntax> targets,
        CancellationToken cancellationToken
    )
    {
        // DocumentEditor applies all replacements atomically against the
        // original tree, so several `return`s never go stale on each other
        // the way sequential ReplaceNode calls would.
        DocumentEditor? editor = await DocumentEditor
            .CreateAsync(document, cancellationToken)
            .ConfigureAwait(false);

        foreach (ExpressionSyntax target in targets)
        {
            editor.ReplaceNode(
                target,
                ToEndpointResult(target)
                    .WithTriviaFrom(target)
                    .WithAdditionalAnnotations(Formatter.Annotation)
            );
        }

        return editor.GetChangedDocument();
    }
}
