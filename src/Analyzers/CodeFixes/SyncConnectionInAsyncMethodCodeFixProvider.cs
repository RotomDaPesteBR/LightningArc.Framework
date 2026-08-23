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
/// Code fix for LARC040 - replaces a synchronous <c>GetConnection()</c> call with
/// <c>await GetConnectionAsync(...).ConfigureAwait(false)</c>. If the enclosing method has a
/// <see cref="CancellationToken"/> parameter, it is threaded through; otherwise the call is
/// generated with no arguments.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(SyncConnectionInAsyncMethodCodeFixProvider)
)]
[Shared]
public class SyncConnectionInAsyncMethodCodeFixProvider : CodeFixProvider
{
    private const string _title = "Use GetConnectionAsync()";
    private const string CancellationTokenMetadataName = "System.Threading.CancellationToken";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [SyncConnectionInAsyncMethodAnalyzer.DiagnosticId];

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

        // Diagnostic location is the whole invocation's span here (not a single token), so
        // FindNode is the right tool — it resolves directly to the InvocationExpressionSyntax.
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
                    UseAsyncConnectionAsync(context.Document, invocation, c),
                equivalenceKey: nameof(SyncConnectionInAsyncMethodCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> UseAsyncConnectionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken
    )
    {
        SemanticModel? semanticModel = await document
            .GetSemanticModelAsync(cancellationToken)
            .ConfigureAwait(false);
        SyntaxNode? root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (semanticModel == null || root == null)
        {
            return document;
        }

        ArgumentListSyntax newArgumentList = ArgumentList();

        MethodDeclarationSyntax? enclosingMethod =
            invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (enclosingMethod != null)
        {
            IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(
                enclosingMethod,
                cancellationToken
            );

            IParameterSymbol? tokenParameter = methodSymbol?.Parameters.FirstOrDefault(p =>
                p.Type.ToDisplayString() == CancellationTokenMetadataName
            );

            if (tokenParameter != null)
            {
                newArgumentList = ArgumentList(
                    SingletonSeparatedList(Argument(IdentifierName(tokenParameter.Name)))
                );
            }
        }

        // Preserve qualification style (bare `GetConnection()` vs `this.GetConnection()`)
        // rather than assuming the call is always unqualified.
        ExpressionSyntax asyncMethodExpression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.WithName(
                IdentifierName("GetConnectionAsync")
            ),
            _ => IdentifierName("GetConnectionAsync"),
        };

        InvocationExpressionSyntax asyncCall = InvocationExpression(
            asyncMethodExpression,
            newArgumentList
        );

        InvocationExpressionSyntax configureAwaitCall = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                asyncCall,
                IdentifierName("ConfigureAwait")
            ),
            ArgumentList(
                SingletonSeparatedList(
                    Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression))
                )
            )
        );

        AwaitExpressionSyntax awaitExpression = AwaitExpression(configureAwaitCall)
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode newRoot = root.ReplaceNode(invocation, awaitExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
