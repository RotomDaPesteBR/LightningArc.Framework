namespace LightningArc.Data.Abstractions.UnitOfWork;

/// <summary>
/// Represents the lifecycle state of an <see cref="IUnitOfWork"/>.
/// </summary>
/// <remarks>
/// <para>Typical transitions:</para>
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="NotStarted"/> -&gt; <see cref="Active"/> via <c>Begin</c>/<c>BeginAsync</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Active"/> -&gt; <see cref="Committed"/> via <c>Commit</c>/<c>CommitAsync</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Active"/> -&gt; <see cref="RolledBack"/> via <c>Rollback</c>/<c>RollbackAsync</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Active"/> -&gt; <see cref="Failed"/> when a commit throws or a rollback could not be confirmed.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Committed"/>, <see cref="RolledBack"/> and <see cref="Failed"/> -&gt; <see cref="Active"/>
/// via a new <c>Begin</c>/<c>BeginAsync</c> (the unit of work can be reused).
/// </description>
/// </item>
/// <item>
/// <description>
/// Any state -&gt; <see cref="Disposed"/> via <c>Dispose</c>/<c>DisposeAsync</c>. This state is terminal.
/// </description>
/// </item>
/// </list>
/// </remarks>
public enum UnitOfWorkState
{
    /// <summary>
    /// No transaction has been started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// A transaction is in progress and can be committed or rolled back.
    /// </summary>
    Active,

    /// <summary>
    /// The last transaction was successfully committed and its resources were released.
    /// </summary>
    Committed,

    /// <summary>
    /// The last transaction was successfully rolled back and its resources were released.
    /// </summary>
    RolledBack,

    /// <summary>
    /// The commit threw an exception, or the rollback could not be confirmed.
    /// The final outcome of the transaction is indeterminate and its resources were released.
    /// </summary>
    Failed,

    /// <summary>
    /// The unit of work was disposed and can no longer be used.
    /// </summary>
    Disposed,
}
