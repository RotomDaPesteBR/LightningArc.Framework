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
/// <c>nullableValueObject?.Value ?? string.Empty</c> when the ValueObject's <c>Value</c> is a
/// <c>string</c>, or <c>nullableValueObject?.ToString() ?? string.Empty</c> otherwise (e.g.
/// <c>Currency</c>, whose <c>Value</c> is <c>decimal</c> and would not compile with the
/// <c>?.Value</c> form).
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
        SemanticModel? semanticModel = await document
            .GetSemanticModelAsync(cancellationToken)
            .ConfigureAwait(false);
        SyntaxNode? root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        ExpressionSyntax nullSafeAccess = UsesStringValue(semanticModel, original)
            // original?.Value
            ? (ExpressionSyntax)
                ConditionalAccessExpression(
                    original.WithoutTrivia(),
                    MemberBindingExpression(IdentifierName("Value"))
                )
            // original?.ToString()
            : ConditionalAccessExpression(
                original.WithoutTrivia(),
                InvocationExpression(MemberBindingExpression(IdentifierName("ToString")))
            );

        // string.Empty
        MemberAccessExpressionSyntax stringEmpty = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            PredefinedType(Token(SyntaxKind.StringKeyword)),
            IdentifierName("Empty")
        );

        // nullSafeAccess ?? string.Empty
        BinaryExpressionSyntax coalesce = BinaryExpression(
                SyntaxKind.CoalesceExpression,
                nullSafeAccess,
                stringEmpty
            )
            .WithTriviaFrom(original)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(original, coalesce);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <summary>
    /// Resolves the expression's ValueObject type through the semantic model and reports
    /// whether its <c>Value</c> property is a <c>string</c>. A missing model, an unresolvable
    /// type, or a missing/non-string <c>Value</c> all yield <c>false</c>, selecting the
    /// always-compilable <c>?.ToString()</c> form.
    /// </summary>
    private static bool UsesStringValue(
        SemanticModel? semanticModel,
        ExpressionSyntax original
    )
    {
        ITypeSymbol? type = semanticModel?.GetTypeInfo(original).Type;
        ITypeSymbol? valueType = type
            ?.GetMembers("Value")
            .OfType<IPropertySymbol>()
            .FirstOrDefault()
            ?.Type;
        return valueType?.SpecialType == SpecialType.System_String;
    }
}
