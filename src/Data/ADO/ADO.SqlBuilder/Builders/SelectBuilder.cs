using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Builders;

/// <summary>
/// A builder for creating SQL SELECT statements with support for WHERE clauses, ORDER BY and Limits.
/// </summary>
public class SelectBuilder(
    string tableName,
    IEnumerable<ColumnDefinition> columns,
    SqlDialect dialect,
    SqlBuilderOptions? options = null
) : SqlStatementBuilder(tableName, columns, dialect, options)
{
    /// <summary>
    /// Builds the SQL SELECT statement.
    /// </summary>
    /// <param name="filterByKey">If true, automatically generates a WHERE clause based on primary keys.</param>
    /// <param name="limit">The maximum number of rows to return. 0 means no limit.</param>
    /// <returns>A string containing the generated SQL SELECT statement.</returns>
    public string Build(bool filterByKey = false, int limit = 0)
    {
        string columnSeparator = Options.Indented ? $",{Environment.NewLine}{Indent}" : ", ";
        string columnList = string.Join(columnSeparator, Columns.Select(c => c.ColumnName));

        // SQL Server TOP
        string topClause =
            (Dialect == SqlDialect.SqlServer && limit > 0) ? $" TOP {limit}" : string.Empty;

        string whereClause = string.Empty;
        if (filterByKey)
        {
            var keys = Columns.Where(c => c.IsKey).ToList();

            if (keys.Count <= 0)
            {
                throw new InvalidOperationException(
                    "Cannot generate WHERE clause because no primary keys were defined in the column definitions."
                );
            }

            whereClause = $"{NewLine}WHERE {BuildKeyWhereClause(keys)}";
        }

        string orderByClause = BuildOrderByClause();

        // Dialect specific limit at the end
        string limitClause = BuildLimitClause(limit);

        return $"SELECT{topClause} {columnList}{NewLine}FROM {TableName}{whereClause}{orderByClause}{limitClause}";
    }

    private string BuildLimitClause(int limit)
    {
        if (limit <= 0)
            return string.Empty;

        return Dialect switch
        {
            SqlDialect.PostgreSQL => $"{NewLine}LIMIT {limit}",
            SqlDialect.Oracle => $"{NewLine}FETCH NEXT {limit} ROWS ONLY",
            _ => string.Empty, // SQL Server handles this via TOP in the SELECT clause
        };
    }

    private string BuildOrderByClause()
    {
        var orderedColumns = Columns
            .Where(c => c.Order != null)
            .OrderBy(c => c.Order!.Priority)
            .ToList();

        if (orderedColumns.Count == 0)
        {
            return string.Empty;
        }

        string orderSeparator = Options.Indented ? $",{Environment.NewLine}{Indent}" : ", ";
        var sortExpressions = orderedColumns.Select(c =>
            $"{c.ColumnName} {(c.Order!.Direction == OrderDirection.Ascending ? "ASC" : "DESC")}"
        );

        return $"{NewLine}ORDER BY {string.Join(orderSeparator, sortExpressions)}";
    }
}

