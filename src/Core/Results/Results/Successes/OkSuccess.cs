using LightningArc.Results.Messages;

namespace LightningArc.Results.Successes;

/// <summary>
/// Represents the success type for an "Ok" operation.
/// </summary>
public sealed class OkSuccess : Success
{
    internal OkSuccess(IMessageProvider? messageProvider)
        : base(100, messageProvider) { }

    /// <inheritdoc/>
    public override Success<TValue> WithValue<TValue>(TValue value) =>
        new OkSuccess<TValue>(value, MessageProvider);
}

/// <summary>
/// Represents the success type for an "Ok" operation with a <typeparamref name="TValue"/> value.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public sealed class OkSuccess<TValue> : Success<TValue>
{
    /// <param name="value">The success value.</param>
    /// <param name="messageProvider">The optional message provider.</param>
    internal OkSuccess(TValue value, IMessageProvider? messageProvider)
        : base(100, messageProvider, value) { }

    internal OkSuccess(int code, IMessageProvider? messageProvider, TValue value) // For custom codes
        : base(code, messageProvider, value) { }

    internal OkSuccess(Success existingSuccess, TValue value)
        : base(existingSuccess, value) { }

    /// <inheritdoc/>
    public override Success<TMappedValue> WithValue<TMappedValue>(TMappedValue value) =>
        new OkSuccess<TMappedValue>(value, MessageProvider);
}
