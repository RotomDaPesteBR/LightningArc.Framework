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
/// Code fix for LARC001 - Wraps .Value access in a TryGetValue check.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultValueUnsafeAccessCodeFixProvider))]
[Shared]
public class ResultValueUnsafeAccessCodeFixProvider : CodeFixProvider
{
	private const string Title = "Use TryGetValue instead";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		ImmutableArray.Create(ResultValueUnsafeAccessAnalyzer.DiagnosticId);

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

		// Walk up to find the MemberAccessExpressionSyntax containing ".Value"
		var memberAccess = node?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>()
			.FirstOrDefault(ma => ma.Name.Span.Start == diagnosticSpan.Start && ma.Name.Identifier.ValueText == "Value");

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
				equivalenceKey: nameof(ResultValueUnsafeAccessCodeFixProvider)),
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

		// Build: result.TryGetValue(out var value)
		var tryGetValueInvocation = InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					resultExpression.WithoutLeadingTrivia().WithoutTrailingTrivia(),
					IdentifierName("TryGetValue")))
			.WithArgumentList(ArgumentList(SeparatedList(new[]
			{
				Argument(DeclarationExpression(
					IdentifierName("var"),
					SingleVariableDesignation(Identifier("value"))))
					.WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
			})));

		// Build the replacement block statement
		var rewritten = RewriteValueAccess(statement, memberAccess);

		var ifStatement = IfStatement(tryGetValueInvocation,
				Block(rewritten.WithLeadingTrivia(Space)))
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot = root.ReplaceNode(statement, ifStatement.WithAdditionalAnnotations(Formatter.Annotation));
		return document.WithSyntaxRoot(newRoot);
	}

	private static StatementSyntax RewriteValueAccess(
		StatementSyntax statement,
		MemberAccessExpressionSyntax valueAccess)
	{
		var rewriter = new ValueAccessRewriter(valueAccess);
		return (StatementSyntax)rewriter.Visit(statement);
	}

	private class ValueAccessRewriter : CSharpSyntaxRewriter
	{
		private readonly MemberAccessExpressionSyntax _valueAccess;

		public ValueAccessRewriter(MemberAccessExpressionSyntax valueAccess)
		{
			_valueAccess = valueAccess;
		}

		public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			if (node.Name.Identifier.ValueText == "Value" &&
				node.Expression.IsEquivalentTo(_valueAccess.Expression))
			{
				return IdentifierName("value")
					.WithLeadingTrivia(node.GetLeadingTrivia())
					.WithTrailingTrivia(node.GetTrailingTrivia());
			}

			return base.VisitMemberAccessExpression(node);
		}
	}
}
