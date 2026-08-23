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
/// Code fix for LARC021 - rewrites <c>nullableValueObject</c> (in a string-typed context) into
/// <c>nullableValueObject?.Value ?? string.Empty</c>.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(NullValueObjectToStringConversionCodeFixProvider)
)]
[Shared]
public class NullValueObjectToStringConversionCodeFixProvider : CodeFixProvider
{
    private const string _title = "Use null-safe access to string value";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [NullValueObjectToStringConversionAnalyzer.DiagnosticId];

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

        // The diagnostic location is always the reported expression itself (declarator
        // initializer value, assignment RHS, or argument expression). FindToken + a
        // position-matched ancestor walk (rather than a bare FindNode) avoids accidentally
        // grabbing a wrapping expression that happens to start at the same position.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindToken(diagnosticSpan.Start).Parent;

        ExpressionSyntax? expression = node
            ?.AncestorsAndSelf()
            .OfType<ExpressionSyntax>()
            .FirstOrDefault(e => e.Span.Start == diagnosticSpan.Start);

        if (expression == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    ApplyNullSafeAccessAsync(context.Document, expression, c),
                equivalenceKey: nameof(NullValueObjectToStringConversionCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> ApplyNullSafeAccessAsync(
        Document document,
        ExpressionSyntax original,
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

        // original?.Value
        ConditionalAccessExpressionSyntax conditionalAccess = ConditionalAccessExpression(
            original.WithoutTrivia(),
            MemberBindingExpression(IdentifierName("Value"))
        );

        // string.Empty
        MemberAccessExpressionSyntax stringEmpty = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            PredefinedType(Token(SyntaxKind.StringKeyword)),
            IdentifierName("Empty")
        );

        // original?.Value ?? string.Empty
        BinaryExpressionSyntax coalesce = BinaryExpression(
                SyntaxKind.CoalesceExpression,
                conditionalAccess,
                stringEmpty
            )
            .WithTriviaFrom(original)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(original, coalesce);
        return document.WithSyntaxRoot(newRoot);
    }
}
