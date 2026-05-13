using LightningArc.Results.Messages;

namespace LightningArc.Results.Successes;

/// <summary>
/// Represents the success type for a "No Content" operation.
/// </summary>
public sealed class NoContentSuccess : Success
{
    internal NoContentSuccess(IMessageProvider? messageProvider)
        : base(103, messageProvider) { }

    /// <inheritdoc/>
    public override Success<TValue> WithValue<TValue>(TValue value) =>
        new NoContentSuccess<TValue>(value, MessageProvider);
}

/// <summary>
/// Represents the success type for a "No Content" operation with a <typeparamref name="TValue"/> value.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
public sealed class NoContentSuccess<TValue> : Success<TValue>
{
    /// <param name="value">The success value.</param>
    /// <param name="messageProvider">The optional message provider.</param>
    internal NoContentSuccess(TValue value, IMessageProvider? messageProvider)
        : base(103, messageProvider, value) { }

    internal NoContentSuccess(Success existingSuccess, TValue value)
        : base(existingSuccess, value) { }

    /// <inheritdoc/>
    public override Success<TMappedValue> WithValue<TMappedValue>(TMappedValue value) =>
        new NoContentSuccess<TMappedValue>(value, MessageProvider);
}
