using LightningArc.Results.Messages;

namespace LightningArc.Results.Successes;

/// <summary>
/// Represents the success type for an "Accepted" operation.
/// </summary>
public sealed class AcceptedSuccess : Success
{
    internal AcceptedSuccess(IMessageProvider? messageProvider)
        : base(102, messageProvider) { }

    /// <inheritdoc/>
    public override Success<TValue> WithValue<TValue>(TValue value) =>
        new AcceptedSuccess<TValue>(value, MessageProvider);
}

/// <summary>
/// Represents the success type for an "Accepted" operation with a <typeparamref name="TValue"/> value.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
public sealed class AcceptedSuccess<TValue> : Success<TValue>
{
    /// <param name="value">The success value.</param>
    /// <param name="messageProvider">The optional message provider.</param>
    internal AcceptedSuccess(TValue value, IMessageProvider? messageProvider)
        : base(102, messageProvider, value) { }

    internal AcceptedSuccess(Success existingSuccess, TValue value)
        : base(existingSuccess, value) { }

    /// <inheritdoc/>
    public override Success<TMappedValue> WithValue<TMappedValue>(TMappedValue value) =>
        new AcceptedSuccess<TMappedValue>(value, MessageProvider);
}
