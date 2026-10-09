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
/// Code fix for LARC020 - wraps the flagged string literal as
/// <c>&lt;VO&gt;.Create("literal")</c>, making the validation explicit instead of
/// relying on the throwing implicit conversion. A single action only; a
/// <c>TryCreate</c>-based reshape is deliberately out of scope.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(ImplicitStringToValueObjectCodeFixProvider)
)]
[Shared]
public class ImplicitStringToValueObjectCodeFixProvider : CodeFixProvider
{
    private const string _title = "Wrap with explicit Create() call";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ImplicitStringToValueObjectAnalyzer.DiagnosticId];

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

        // Diagnostic location is the string literal itself.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        LiteralExpressionSyntax? literal =
            node as LiteralExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().FirstOrDefault();

        if (literal == null || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        SemanticModel? semanticModel = await context
            .Document.GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (semanticModel == null)
        {
            return;
        }

        string? typeName = ResolveValueObjectName(literal, semanticModel);
        if (typeName == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    WrapWithCreateAsync(context.Document, literal, typeName, c),
                equivalenceKey: nameof(ImplicitStringToValueObjectCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    /// <summary>
    /// Resolves the destination ValueObject type name for the flagged literal,
    /// mirroring the three syntactic shapes the analyzer reports on (variable
    /// declarator, assignment, argument).
    /// </summary>
    private static string? ResolveValueObjectName(
        LiteralExpressionSyntax literal,
        SemanticModel semanticModel
    )
    {
        // Case 1: Email email = "literal";
        if (
            literal.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
        )
        {
            ISymbol? declared = semanticModel.GetDeclaredSymbol(declarator);
            ITypeSymbol? type = declared switch
            {
                ILocalSymbol local => local.Type,
                IFieldSymbol field => field.Type,
                _ => null,
            };

            if (type != null && ValueObjectTypeRecognizer.IsValueObjectType(type))
            {
                return type.Name;
            }

            return null;
        }

        // Case 2: email = "literal";
        if (
            literal.Parent is AssignmentExpressionSyntax assignment
            && assignment.Right == literal
        )
        {
            ITypeSymbol? converted = semanticModel.GetTypeInfo(assignment.Left).ConvertedType;
            if (converted != null && ValueObjectTypeRecognizer.IsValueObjectType(converted))
            {
                return converted.Name;
            }

            return null;
        }

        // Case 3: Process("literal");
        if (literal.Parent is ArgumentSyntax)
        {
            ITypeSymbol? converted = semanticModel.GetTypeInfo(literal).ConvertedType;
            if (converted != null && ValueObjectTypeRecognizer.IsValueObjectType(converted))
            {
                return converted.Name;
            }

            return null;
        }

        return null;
    }

    private static async Task<Document> WrapWithCreateAsync(
        Document document,
        LiteralExpressionSyntax literal,
        string typeName,
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

        InvocationExpressionSyntax createCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(typeName),
                    IdentifierName("Create")
                ),
                ArgumentList(
                    SingletonSeparatedList(Argument(literal.WithoutTrivia()))
                )
            )
            .WithTriviaFrom(literal)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(literal, createCall);
        return document.WithSyntaxRoot(newRoot);
    }
}
