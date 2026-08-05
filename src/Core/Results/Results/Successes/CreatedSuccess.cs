using LightningArc.Results.Messages;

namespace LightningArc.Results.Successes;

/// <summary>
/// Represents the success type for a "Created" operation.
/// </summary>
public sealed class CreatedSuccess : Success
{
    internal CreatedSuccess(IMessageProvider? messageProvider)
        : base(101, messageProvider) { }

    /// <inheritdoc/>
    public override Success<TValue> WithValue<TValue>(TValue value) =>
        new CreatedSuccess<TValue>(value, MessageProvider);
}

/// <summary>
/// Represents the success type for a "Created" operation with a <typeparamref name="TValue"/> value.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
public sealed class CreatedSuccess<TValue> : Success<TValue>
{
    /// <param name="value">The success value.</param>
    /// <param name="messageProvider">The optional message provider.</param>
    internal CreatedSuccess(TValue value, IMessageProvider? messageProvider)
        : base(101, messageProvider, value) { }

    internal CreatedSuccess(Success existingSuccess, TValue value)
        : base(existingSuccess, value) { }

    /// <inheritdoc/>
    public override Success<TMappedValue> WithValue<TMappedValue>(TMappedValue value) =>
        new CreatedSuccess<TMappedValue>(value, MessageProvider);
}
