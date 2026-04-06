using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC002 - Wraps .Error access in a TryGetError check.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultErrorUnsafeAccessCodeFixProvider))]
[Shared]
public class ResultErrorUnsafeAccessCodeFixProvider : CodeFixProvider
{
	private const string Title = "Use TryGetError instead";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		ImmutableArray.Create(ResultErrorUnsafeAccessAnalyzer.DiagnosticId);

	public sealed override FixAllProvider GetFixAllProvider() =>
		WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null)
			return;

		// Try to find the member access at the diagnostic location
		var diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
		var node = root.FindToken(diagnosticSpan.Start).Parent;

		// Walk up to find the MemberAccessExpressionSyntax containing ".Error"
		var memberAccess = node?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>()
			.FirstOrDefault(ma => ma.Name.Span.Start == diagnosticSpan.Start && ma.Name.Identifier.ValueText == "Error");

		if (memberAccess == null)
			return;

		// Find the enclosing statement
		var statement = memberAccess.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
		if (statement == null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: Title,
				createChangedDocument: c => ApplyFixAsync(context.Document, root, memberAccess, statement, c),
				equivalenceKey: nameof(ResultErrorUnsafeAccessCodeFixProvider)),
			diagnostics: context.Diagnostics);
	}

	private static async Task<Document> ApplyFixAsync(
		Document document,
		SyntaxNode root,
		MemberAccessExpressionSyntax memberAccess,
		StatementSyntax statement,
		CancellationToken cancellationToken)
	{
		var resultExpression = memberAccess.Expression;

		// Build: result.TryGetError(out var err)
		var tryGetErrorInvocation = InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					resultExpression.WithoutLeadingTrivia().WithoutTrailingTrivia(),
					IdentifierName("TryGetError")))
			.WithArgumentList(ArgumentList(SeparatedList(new[]
			{
				Argument(DeclarationExpression(
					IdentifierName("var"),
					SingleVariableDesignation(Identifier("err"))))
					.WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
			})));

		// Build the replacement block statement
		var rewritten = RewriteErrorAccess(statement, memberAccess);

		var ifStatement = IfStatement(tryGetErrorInvocation,
				Block(rewritten.WithLeadingTrivia(Space)))
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot = root.ReplaceNode(statement, ifStatement.WithAdditionalAnnotations(Formatter.Annotation));
		return document.WithSyntaxRoot(newRoot);
	}

	private static StatementSyntax RewriteErrorAccess(
		StatementSyntax statement,
		MemberAccessExpressionSyntax errorAccess)
	{
		var rewriter = new ErrorAccessRewriter(errorAccess);
		return (StatementSyntax)rewriter.Visit(statement);
	}

	private class ErrorAccessRewriter : CSharpSyntaxRewriter
	{
		private readonly MemberAccessExpressionSyntax _errorAccess;

		public ErrorAccessRewriter(MemberAccessExpressionSyntax errorAccess)
		{
			_errorAccess = errorAccess;
		}

		public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			if (node.Name.Identifier.ValueText == "Error" &&
				node.Expression.IsEquivalentTo(_errorAccess.Expression))
			{
				return IdentifierName("err")
					.WithLeadingTrivia(node.GetLeadingTrivia())
					.WithTrailingTrivia(node.GetTrailingTrivia());
			}

			return base.VisitMemberAccessExpression(node);
		}
	}
}
