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
/// Code fix for LARC043 - replaces the null/default argument of
/// <c>ReleaseConnection(...)</c> with the single local variable assigned from
/// <c>GetConnection()</c> / <c>await GetConnectionAsync(...)</c> in the enclosing method.
/// Offered only when exactly one such candidate exists; with zero or multiple
/// candidates no fix is offered (guessing would risk releasing the wrong connection).
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(ReleaseConnectionNullArgumentCodeFixProvider)
)]
[Shared]
public class ReleaseConnectionNullArgumentCodeFixProvider : CodeFixProvider
{
    private const string _title = "Pass the acquired connection instead of null";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ReleaseConnectionNullArgumentAnalyzer.DiagnosticId];

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

        // Diagnostic location is the whole invocation's span, so FindNode
        // resolves directly to the InvocationExpressionSyntax.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        InvocationExpressionSyntax? invocation =
            node as InvocationExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

        if (invocation?.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        SyntaxNode? scope = invocation
            .Ancestors()
            .FirstOrDefault(a =>
                a
                    is MethodDeclarationSyntax
                        or ConstructorDeclarationSyntax
                        or LocalFunctionStatementSyntax
            );

        if (scope == null)
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

        string? candidate = FindSingleConnectionLocal(scope, semanticModel);
        if (candidate == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c =>
                    ReplaceNullArgumentAsync(
                        context.Document,
                        invocation,
                        candidate,
                        c
                    ),
                equivalenceKey: nameof(ReleaseConnectionNullArgumentCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    /// <summary>
    /// Finds the single local variable assigned from a <c>GetConnection()</c> /
    /// <c>GetConnectionAsync(...)</c> call in <paramref name="scope"/>. Returns
    /// <c>null</c> unless exactly one distinct variable qualifies.
    /// </summary>
    private static string? FindSingleConnectionLocal(
        SyntaxNode scope,
        SemanticModel semanticModel
    )
    {
        HashSet<string> candidates = [];

        foreach (VariableDeclaratorSyntax declarator in scope
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
        )
        {
            if (
                declarator.Initializer?.Value is ExpressionSyntax initializer
                && ContainsConnectionAcquisition(initializer, semanticModel)
            )
            {
                candidates.Add(declarator.Identifier.ValueText);
            }
        }

        foreach (AssignmentExpressionSyntax assignment in scope
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
        )
        {
            if (
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                && assignment.Left is IdentifierNameSyntax target
                && ContainsConnectionAcquisition(assignment.Right, semanticModel)
            )
            {
                candidates.Add(target.Identifier.ValueText);
            }
        }

        return candidates.Count == 1 ? candidates.Single() : null;
    }

    private static bool ContainsConnectionAcquisition(
        ExpressionSyntax expression,
        SemanticModel semanticModel
    )
    {
        return expression
            .DescendantNodesAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .Any(inv => IsConnectionAcquisition(inv, semanticModel));
    }

    private static bool IsConnectionAcquisition(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel
    )
    {
        string methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => string.Empty,
        };

        if (methodName != "GetConnection" && methodName != "GetConnectionAsync")
        {
            return false;
        }

        // Guard against an unrelated same-named method returning something that
        // is not a connection: only accept calls producing a DbConnection
        // (unwrapping Task/ValueTask for the async overload).
        ITypeSymbol? returnType = semanticModel.GetTypeInfo(invocation).Type;
        if (returnType == null)
        {
            return false;
        }

        if (
            returnType is INamedTypeSymbol named
            && named.IsGenericType
            && named.TypeArguments.Length == 1
            && (named.Name == "Task" || named.Name == "ValueTask")
        )
        {
            returnType = named.TypeArguments[0];
        }

        INamedTypeSymbol? dbConnectionType = semanticModel.Compilation.GetTypeByMetadataName(
            "System.Data.Common.DbConnection"
        );

        if (dbConnectionType == null)
        {
            return false;
        }

        for (ITypeSymbol? current = returnType; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, dbConnectionType))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<Document> ReplaceNullArgumentAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        string variableName,
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

        ArgumentSyntax oldArgument = invocation.ArgumentList.Arguments[0];
        ArgumentSyntax newArgument = oldArgument
            .WithExpression(
                IdentifierName(variableName).WithTriviaFrom(oldArgument.Expression)
            )
            .WithAdditionalAnnotations(Formatter.Annotation);

        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(
            invocation.ArgumentList.WithArguments(SingletonSeparatedList(newArgument))
        );

        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
