using System.Data.Common;

namespace LightningArc.Data.ADO.Factories;

/// <summary>
/// Defines the interface for a database connection factory,
/// responsible for providing <see cref="DbConnection"/> instances.
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Gets and returns a new or existing open database connection.
    /// </summary>
    /// <returns>An open instance of <see cref="DbConnection"/>.</returns>
    DbConnection GetConnection();
}

