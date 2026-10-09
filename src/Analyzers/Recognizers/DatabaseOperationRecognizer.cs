using Microsoft.CodeAnalysis;

namespace LightningArc.Analyzers;

/// <summary>
/// Shared predicate identifying a database-operation call: a Dapper extension
/// method (by name) or a method declared on <c>DbConnection</c>/<c>IDbConnection</c>.
/// Extracted from <c>RepositoryTransactionAnalyzer</c> (LARC041) so LARC045 can
/// reuse it without duplication.
/// </summary>
internal static class DatabaseOperationRecognizer
{
    private static readonly string[] DapperMethods =
    [
        "Query",
        "QueryAsync",
        "Execute",
        "ExecuteAsync",
        "QuerySingle",
        "QuerySingleAsync",
        "QueryFirst",
        "QueryFirstAsync",
        "QueryFirstOrDefault",
        "QueryFirstOrDefaultAsync",
        "QuerySingleOrDefault",
        "QuerySingleOrDefaultAsync",
    ];

    /// <summary>
    /// Determines whether <paramref name="method"/> is a database-operation call.
    /// </summary>
    public static bool IsDatabaseOperation(IMethodSymbol method)
    {
        // Check Dapper extension methods
        if (DapperMethods.Contains(method.Name))
        {
            return true;
        }

        // Check DbConnection methods
        if (
            method.ContainingType != null
            && (
                method.ContainingType.Name == "DbConnection"
                || method.ContainingType.Name == "IDbConnection"
            )
        )
        {
            return true;
        }

        return false;
    }
}
