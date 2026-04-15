using System.Text;
using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Builders;

/// <summary>
/// A builder for creating SQL INSERT statements with support for database-generated values.
/// </summary>
/// <param name="tableName">The name of the target table.</param>
/// <param name="columns">The collection of column definitions.</param>
/// <param name="dialect">The target SQL dialect.</param>
/// <param name="options">The formatting options.</param>
public class InsertBuilder(
    string tableName,
    IEnumerable<ColumnDefinition> columns,
    SqlDialect dialect,
    SqlBuilderOptions? options = null
) : SqlStatementBuilder(tableName, columns, dialect, options)
{
    /// <summary>
    /// Builds the SQL INSERT statement.
    /// </summary>
    /// <param name="returnGeneratedValues">If true, generates a clause to return database-generated values (OUTPUT/RETURNING).</param>
    /// <returns>A string containing the generated SQL INSERT statement.</returns>
    /// <remarks>
    /// This builder automatically excludes database-generated and computed columns from the INSERT list.
    /// It also supports dialect-specific syntax for returning generated values (e.g., OUTPUT in SQL Server, RETURNING in PostgreSQL and Oracle).
    /// </remarks>
    public string Build(bool returnGeneratedValues = false)
    {
        var insertColumns = Columns
            .Where(c => c is { IsDatabaseGenerated: false, IsComputed: false })
            .ToList();

        var outputColumns = Columns.Where(c => c.IsDatabaseGenerated).ToList();

        string columnSeparator = Options.Indented ? $",{Environment.NewLine}{Indent}" : ", ";

        string columnNames = string.Join(columnSeparator, insertColumns.Select(c => c.ColumnName));
        string parameterNames = string.Join(
            columnSeparator,
            insertColumns.Select(c => GetParameter(c.PropertyName))
        );

        StringBuilder sqlBuilder = new();

        if (Options.Indented)
        {
            sqlBuilder.Append($"INSERT INTO {TableName}{Environment.NewLine}");
            sqlBuilder.Append($"({Environment.NewLine}{Indent}{columnNames}{Environment.NewLine})");
        }
        else
        {
            sqlBuilder.Append($"INSERT INTO {TableName} ({columnNames})");
        }

        // SQL Server OUTPUT
        if (returnGeneratedValues && Dialect == SqlDialect.SqlServer && outputColumns.Count > 0)
        {
            string outputList = string.Join(
                columnSeparator,
                outputColumns.Select(c => $"INSERTED.{c.ColumnName}")
            );
            sqlBuilder.Append(
                Options.Indented
                    ? $"{Environment.NewLine}OUTPUT {outputList}"
                    : $" OUTPUT {outputList}"
            );
        }

        if (Options.Indented)
        {
            sqlBuilder.Append($"{Environment.NewLine}VALUES{Environment.NewLine}");
            sqlBuilder.Append(
                $"({Environment.NewLine}{Indent}{parameterNames}{Environment.NewLine})"
            );
        }
        else
        {
            sqlBuilder.Append($" VALUES ({parameterNames})");
        }

        // RETURNING (Postgres/Oracle)
        if (!returnGeneratedValues || outputColumns.Count <= 0)
            return sqlBuilder.ToString();

        string returningList = string.Join(
            columnSeparator,
            outputColumns.Select(c => c.ColumnName)
        );

        switch (Dialect)
        {
            case SqlDialect.PostgreSQL:
                sqlBuilder.Append(
                    Options.Indented
                        ? $"{Environment.NewLine}RETURNING {returningList}"
                        : $" RETURNING {returningList}"
                );
                break;
            case SqlDialect.Oracle:
            {
                string intoList = string.Join(
                    columnSeparator,
                    outputColumns.Select(c => GetParameter(c.PropertyName) + "_Return")
                );
                sqlBuilder.Append(
                    Options.Indented
                        ? $"{Environment.NewLine}RETURNING {returningList}{Environment.NewLine}INTO {intoList}"
                        : $" RETURNING {returningList} INTO {intoList}"
                );

                break;
            }
            case SqlDialect.SqlServer:
                break;
        }

        return sqlBuilder.ToString();
    }
}

