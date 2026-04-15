using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Builders;

/// <summary>
/// A builder for creating SQL UPDATE statements based on primary key columns.
/// </summary>
/// <param name="tableName">The name of the target table.</param>
/// <param name="columns">The collection of column definitions.</param>
/// <param name="dialect">The target SQL dialect.</param>
/// <param name="options">The formatting options.</param>
public class UpdateBuilder(
    string tableName,
    IEnumerable<ColumnDefinition> columns,
    SqlDialect dialect,
    SqlBuilderOptions? options = null
) : SqlStatementBuilder(tableName, columns, dialect, options)
{
    /// <summary>
    /// Builds the SQL UPDATE statement.
    /// </summary>
    /// <returns>A string containing the generated SQL UPDATE statement.</returns>
    /// <remarks>
    /// This builder automatically excludes primary keys, database-generated columns, and computed columns from the SET clause.
    /// The WHERE clause is generated using the primary key columns.
    /// </remarks>
    public string Build()
    {
        var keys = Columns.Where(c => c.IsKey);

        var updateColumns = Columns.Where(c =>
            c is { IsKey: false, IsDatabaseGenerated: false, IsComputed: false }
        );

        string setSeparator = Options.Indented ? $",{Environment.NewLine}{Indent}" : ", ";
        string setClause = string.Join(
            setSeparator,
            updateColumns.Select(c => $"{c.ColumnName} = {GetParameter(c.PropertyName)}")
        );

        string whereClause = BuildKeyWhereClause(keys);

        return $"UPDATE {TableName}{NewLine}SET {setClause}{NewLine}WHERE {whereClause}";
    }
}

