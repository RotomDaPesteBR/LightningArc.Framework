namespace LightningArc.Data.Abstractions.UnitOfWork;

/// <summary>
/// Provides convenience members for querying groups of <see cref="UnitOfWorkState"/> values.
/// </summary>
public static class UnitOfWorkStateExtensions
{
    /// <param name="state">The state being evaluated.</param>
    extension(UnitOfWorkState state)
    {
        /// <summary>
        /// Gets a value indicating whether a transaction is in progress,
        /// which means it can still be committed or rolled back.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the state is <see cref="UnitOfWorkState.Active"/>;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IsActive => state is UnitOfWorkState.Active;

        /// <summary>
        /// Gets a value indicating whether the last transaction has finished and its resources were released,
        /// regardless of the outcome.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the state is <see cref="UnitOfWorkState.Committed"/>,
        /// <see cref="UnitOfWorkState.RolledBack"/> or <see cref="UnitOfWorkState.Failed"/>;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCompleted =>
            state
                is UnitOfWorkState.Committed
                    or UnitOfWorkState.RolledBack
                    or UnitOfWorkState.Failed;
    }
}
