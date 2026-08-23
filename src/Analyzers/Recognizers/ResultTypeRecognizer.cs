using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// Resolved LightningArc.Results type symbols for a single compilation, with instance methods
/// for recognizing whether a candidate type is one of them.
/// </summary>
/// <remarks>
/// Usage pattern — resolve once per compilation, reuse across syntax-node callbacks:
/// <code>
/// public override void Initialize(AnalysisContext context)
/// {
///     context.RegisterCompilationStartAction(compilationContext =>
///     {
///         ResultTypeSymbols symbols = ResultTypeSymbols.Resolve(compilationContext.Compilation);
///         if (!symbols.IsAvailable) return; // compilation doesn't reference LightningArc.Results
///
///         compilationContext.RegisterSyntaxNodeAction(
///             nodeContext => Analyze(nodeContext, symbols),
///             SyntaxKind.SimpleMemberAccessExpression);
///     });
/// }
/// </code>
/// </remarks>
internal readonly struct ResultTypeRecognizer(
    INamedTypeSymbol? resultType,
    INamedTypeSymbol? errorType,
    INamedTypeSymbol? aggregateErrorType
)
{
    private const string _resultMetadataName = "LightningArc.Results.Result";
    private const string _errorMetadataName = "LightningArc.Results.Error";
    private const string _aggregateErrorMetadataName = "LightningArc.Results.AggregateError";

    public INamedTypeSymbol? ResultType { get; } = resultType;
    public INamedTypeSymbol? ErrorType { get; } = errorType;
    public INamedTypeSymbol? AggregateErrorType { get; } = aggregateErrorType;

    /// <summary>
    /// True if at least <see cref="ResultType"/> and <see cref="ErrorType"/> resolved — the
    /// minimum needed for any Result-domain rule to function meaningfully.
    /// <see cref="AggregateErrorType"/> is treated as optional since not every rule needs it.
    /// </summary>
    public bool IsAvailable => ResultType != null && ErrorType != null;

    /// <summary>
    /// Resolves the known LightningArc.Results type symbols for a given compilation. Call once
    /// per compilation — typically inside <see cref="AnalysisContext.RegisterCompilationStartAction"/>
    /// — and pass the result down to syntax-node callbacks. Resolving via
    /// <see cref="Compilation.GetTypeByMetadataName"/> repeatedly per node is unnecessary work,
    /// and checking <see cref="IsAvailable"/> gives every analyzer using this a free, correct
    /// way to bail out entirely when the compilation being analyzed doesn't reference
    /// LightningArc.Results at all — the same gate that fixed LARC032's false positive outside
    /// ASP.NET Core contexts, generalized.
    /// </summary>
    public static ResultTypeRecognizer Resolve(Compilation compilation) =>
        new(
            compilation.GetTypeByMetadataName(_resultMetadataName),
            compilation.GetTypeByMetadataName(_errorMetadataName),
            compilation.GetTypeByMetadataName(_aggregateErrorMetadataName)
        );

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is <c>Result</c> or any
    /// <c>Result&lt;TValue&gt;</c> instantiation, by walking the base-type chain rather than
    /// comparing bare names or a nonexistent interface.
    /// </summary>
    public bool IsResultType(ITypeSymbol? candidate) =>
        ResultType != null && WalksTo(candidate, ResultType);

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is <c>Error</c> or any type deriving
    /// from it (e.g. <c>AggregateError</c>), by walking the base-type chain.
    /// </summary>
    public bool IsErrorType(ITypeSymbol? candidate) =>
        ErrorType != null && WalksTo(candidate, ErrorType);

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is specifically <c>AggregateError</c>
    /// (an exact match, not "any Error") — for rules that care about the distinction, e.g. a
    /// rule reasoning about flattening or multi-error scenarios specifically.
    /// </summary>
    public bool IsAggregateErrorType(ITypeSymbol? candidate) =>
        AggregateErrorType != null
        && SymbolEqualityComparer.Default.Equals(candidate, AggregateErrorType);

    private static bool WalksTo(ITypeSymbol? candidate, INamedTypeSymbol target)
    {
        for (ITypeSymbol? current = candidate; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, target))
            {
                return true;
            }
        }

        return false;
    }
}
