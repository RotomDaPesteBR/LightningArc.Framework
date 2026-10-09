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
/// Code fix for LARC005 - rewrites <c>return &lt;newError&gt;;</c> to
/// <c>return &lt;guard&gt;.Error + &lt;newError&gt;;</c> so the original failure is
/// preserved instead of shadowed. The guard is resolved with the same
/// <see cref="FailureGuardResolver"/> logic the analyzer uses — never re-derived.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(ResultErrorShadowingCodeFixProvider)
)]
[Shared]
public class ResultErrorShadowingCodeFixProvider : CodeFixProvider
{
    private const string _title = "Preserve the original error";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ResultErrorShadowingAnalyzer.DiagnosticId];

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

        // Diagnostic location is the returned new-error expression. Anchor on the
        // enclosing return/arrow instead of the raw span so tie-breaks resolve
        // to the statement being rewritten.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        SyntaxNode? holder = node
            ?.AncestorsAndSelf()
            .FirstOrDefault(n =>
                n is ReturnStatementSyntax or ArrowExpressionClauseSyntax
            );

        ExpressionSyntax? returned = holder switch
        {
            ReturnStatementSyntax ret => ret.Expression,
            ArrowExpressionClauseSyntax arrow => arrow.Expression,
            _ => null,
        };

        if (holder == null || returned == null)
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

        ResultTypeRecognizer recognizer = ResultTypeRecognizer.Resolve(
            semanticModel.Compilation
        );

        if (!recognizer.IsAvailable)
        {
            return;
        }

        ImmutableArray<ISymbol> guards = FailureGuardResolver.FindActiveFailureGuards(
            returned,
            semanticModel,
            recognizer
        );

        if (guards.Length == 0)
        {
            return;
        }

        // Only Error-typed returns can be combined with `+`: there is no
        // `Error + Result` overload, so a `Result.Failure(...)` return is
        // deliberately left without a fix.
        ITypeSymbol? returnedType = semanticModel.GetTypeInfo(returned).Type;
        if (returnedType == null || !recognizer.IsErrorType(returnedType))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    CombineWithGuardAsync(
                        context.Document,
                        holder,
                        returned,
                        guards,
                        recognizer,
                        c
                    ),
                equivalenceKey: nameof(ResultErrorShadowingCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private static async Task<Document> CombineWithGuardAsync(
        Document document,
        SyntaxNode holder,
        ExpressionSyntax returned,
        ImmutableArray<ISymbol> guards,
        ResultTypeRecognizer recognizer,
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

        // Chain every unconsumed guard left-associatively:
        // return g1.Error + g2.Error + <newError>;
        ExpressionSyntax combined = GuardAccess(guards[0], recognizer);
        for (int i = 1; i < guards.Length; i++)
        {
            combined = BinaryExpression(
                SyntaxKind.AddExpression,
                combined,
                GuardAccess(guards[i], recognizer)
            );
        }

        combined = BinaryExpression(
            SyntaxKind.AddExpression,
            combined,
            returned.WithoutTrivia()
        )
            .WithTriviaFrom(returned)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode replacement = holder switch
        {
            ReturnStatementSyntax ret => ret.WithExpression(combined),
            ArrowExpressionClauseSyntax arrow => arrow.WithExpression(combined),
            _ => holder,
        };

        SyntaxNode newRoot = root.ReplaceNode(holder, replacement);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <summary>
    /// The expression that references a guard's error: <c>guard.Error</c> for
    /// <c>Result</c>-typed guards, the bare guard for guards that already are
    /// an <c>Error</c> (where <c>.Error</c> would not compile).
    /// </summary>
    private static ExpressionSyntax GuardAccess(ISymbol guard, ResultTypeRecognizer recognizer)
    {
        IdentifierNameSyntax name = IdentifierName(guard.Name);

        if (recognizer.IsErrorType(guard.GetSymbolType()))
        {
            return name;
        }

        return MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            name,
            IdentifierName("Error")
        );
    }
}
