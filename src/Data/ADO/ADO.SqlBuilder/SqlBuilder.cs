using LightningArc.Data.ADO.SqlBuilder.Builders;
using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder;

/// <summary>
/// Provides a centralized entry point for building various SQL CRUD statements.
/// </summary>
/// <param name="tableName">The name of the database table.</param>
/// <param name="columns">The collection of column definitions for the table.</param>
/// <param name="dialect">The target SQL database dialect.</param>
/// <param name="options">Optional formatting options for the generated SQL.</param>
public class SqlBuilder(
    string tableName,
    IEnumerable<ColumnDefinition> columns,
    SqlDialect dialect,
    SqlBuilderOptions? options = null
)
{
    private readonly SqlBuilderOptions _options = options ?? new SqlBuilderOptions();

    /// <summary>
    /// Gets a builder for generating SELECT statements.
    /// </summary>
    public SelectBuilder Select => new(tableName, columns, dialect, _options);

    /// <summary>
    /// Gets a builder for generating INSERT statements.
    /// </summary>
    public InsertBuilder Insert => new(tableName, columns, dialect, _options);

    /// <summary>
    /// Gets a builder for generating UPDATE statements.
    /// </summary>
    public UpdateBuilder Update => new(tableName, columns, dialect, _options);

    /// <summary>
    /// Gets a builder for generating DELETE statements.
    /// </summary>
    public DeleteBuilder Delete => new(tableName, columns, dialect, _options);
}

