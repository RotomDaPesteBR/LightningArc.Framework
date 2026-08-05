using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Definitions;

/// <summary>
/// Defines the sorting priority and direction for a column.
/// </summary>
/// <param name="Priority">The priority of the sort (lower values are applied first).</param>
/// <param name="Direction">The direction of the sort (Ascending or Descending).</param>
public record OrderDefinition(int Priority, OrderDirection Direction);

