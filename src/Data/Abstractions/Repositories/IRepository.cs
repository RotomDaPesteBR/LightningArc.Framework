namespace LightningArc.Data.Abstractions.Repositories;

/// <summary>
/// A base marker interface for all repositories in the system.
/// </summary>
public interface IRepository
{
}

/// <summary>
/// Defines a generic repository for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of the domain entity.</typeparam>
public interface IRepository<TEntity> : IRepository
    where TEntity : class
{
    // Basic agnostic CRUD operations could be defined here in the future
}

