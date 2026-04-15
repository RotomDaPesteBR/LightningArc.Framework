using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Builders;

/// <summary>
/// A builder for creating SQL DELETE statements based on primary key columns.
/// </summary>
/// <param name="tableName">The name of the target table.</param>
/// <param name="columns">The collection of column definitions.</param>
/// <param name="dialect">The target SQL dialect.</param>
/// <param name="options">The formatting options.</param>
public class DeleteBuilder(
    string tableName,
    IEnumerable<ColumnDefinition> columns,
    SqlDialect dialect,
    SqlBuilderOptions? options = null
) : SqlStatementBuilder(tableName, columns, dialect, options)
{
    /// <summary>
    /// Builds the SQL DELETE statement.
    /// </summary>
    /// <returns>A string containing the generated SQL DELETE statement.</returns>
    /// <remarks>
    /// The statement is generated using the columns defined as primary keys in the <see cref="ColumnDefinition"/> collection.
    /// </remarks>
    public string Build()
    {
        var keys = Columns.Where(c => c.IsKey);
        string whereClause = BuildKeyWhereClause(keys);

        return $"DELETE FROM {TableName}{NewLine}WHERE {whereClause}";
    }
}

