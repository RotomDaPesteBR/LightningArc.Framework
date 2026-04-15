using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Builders;

/// <summary>
/// Provides a base class for building SQL statements with dialect-specific logic.
/// </summary>
public abstract class SqlStatementBuilder
{
    /// <summary>
    /// Gets the name of the target table.
    /// </summary>
    protected string TableName { get; }

    /// <summary>
    /// Gets the target SQL dialect.
    /// </summary>
    protected SqlDialect Dialect { get; }

    /// <summary>
    /// Gets the formatting options for the generated SQL.
    /// </summary>
    protected SqlBuilderOptions Options { get; }

    /// <summary>
    /// Gets the collection of column definitions for the statement.
    /// </summary>
    protected IEnumerable<ColumnDefinition> Columns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStatementBuilder"/> class.
    /// </summary>
    /// <param name="tableName">The name of the target table.</param>
    /// <param name="columns">The collection of column definitions.</param>
    /// <param name="dialect">The target SQL dialect.</param>
    /// <param name="options">The formatting options.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided SQL dialect is not supported.</exception>
    protected SqlStatementBuilder(
        string tableName,
        IEnumerable<ColumnDefinition> columns,
        SqlDialect dialect,
        SqlBuilderOptions? options = null
    )
    {
#if NETSTANDARD2_0
        if (!Enum.IsDefined(typeof(SqlDialect), dialect))
#else
        if (!Enum.IsDefined(dialect))
#endif
        {
            throw new ArgumentOutOfRangeException(
                nameof(dialect),
                dialect,
                "The provided SQL dialect is not supported."
            );
        }

        TableName = tableName;
        Columns = columns;
        Dialect = dialect;
        Options = options ?? new SqlBuilderOptions();
    }

    /// <summary>
    /// Gets a new line string if indentation is enabled; otherwise, a single space.
    /// </summary>
    protected string NewLine => Options.Indented ? Environment.NewLine : " ";

    /// <summary>
    /// Gets the indentation string based on the configured indent size if indentation is enabled.
    /// </summary>
    protected string Indent =>
        Options.Indented ? new string(' ', Options.IndentSize) : string.Empty;

    /// <summary>
    /// Formats a property name as a database parameter using the correct prefix for the current dialect.
    /// </summary>
    /// <param name="propertyName">The name of the property to format as a parameter.</param>
    /// <returns>A string representing the database parameter (e.g., @Prop or :Prop).</returns>
    protected string GetParameter(string propertyName)
    {
        string prefix = Dialect == SqlDialect.SqlServer ? "@" : ":";
        return $"{prefix}{propertyName}";
    }

    /// <summary>
    /// Builds a SQL WHERE clause for the primary key columns.
    /// </summary>
    /// <param name="keys">The collection of column definitions representing keys.</param>
    /// <returns>A string representing the WHERE clause fragment.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no key columns are defined for a statement that requires them.</exception>
    protected string BuildKeyWhereClause(IEnumerable<ColumnDefinition> keys)
    {
        string separator = Options.Indented ? $"{Environment.NewLine}{Indent}AND " : " AND ";

        var conditions = keys.Select(key => $"{key.ColumnName} = {GetParameter(key.PropertyName)}")
            .ToList();

        return conditions.Count == 0
            ? throw new InvalidOperationException(
                "Cannot build a statement that requires keys without any key defined."
            )
            : string.Join(separator, conditions);
    }
}

