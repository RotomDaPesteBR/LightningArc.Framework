namespace LightningArc.Data.ADO.SqlBuilder.Definitions;

/// <summary>
/// Configuration options for the SQL query generation process.
/// </summary>
public record SqlBuilderOptions
{
    /// <summary>
    /// Gets a value indicating whether the generated SQL should be formatted with indentation and new lines.
    /// </summary>
    public bool Indented { get; init; } = false;

    /// <summary>
    /// Gets the number of spaces to use for each level of indentation.
    /// </summary>
    public int IndentSize { get; init; } = 4;
}

