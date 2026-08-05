using System.Globalization;
using LightningArc.Results.Messages;
using LightningArc.Results.Successes;

namespace LightningArc.Results;

/// <summary>
/// Represents a generic success result.
/// This base class is used to standardize success responses, allowing subclasses
/// to define specific types of success.
/// </summary>
public abstract class Success : IEquatable<Success>
{
    /// <summary>
    /// Gets the numeric code of the success.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets the message provider for this success.
    /// </summary>
    internal readonly IMessageProvider? MessageProvider;

    /// <summary>
    /// Gets the optional success message.
    /// </summary>
    public string? Message => MessageProvider?.GetMessage(CultureInfo.CurrentCulture);

    /// <summary>
    /// Provides a fluent entry point to create custom success
    /// through the <see cref="Hook"/> mechanism.
    /// </summary>
    /// <value>
    /// A new instance of the <see cref="Hook"/> class.
    /// </value>
    public static Hook Of => new();

    /// <summary>
    /// Protected constructor to initialize the base <see cref="Success"/> instance with a message provider.
    /// </summary>
    /// <param name="code">The numeric code of the success.</param>
    /// <param name="messageProvider">The message provider for this success.</param>
    protected Success(int code, IMessageProvider? messageProvider)
    {
        if (code <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "Success code must be a positive integer."
            );
        }

        Code = code;
        MessageProvider = messageProvider;
    }

    /// <param name="code">The numeric code of the success.</param>
    /// <param name="message">The optional literal success message. Will be used to create a LiteralMessageProvider.</param>
    protected Success(int code, string? message)
    {
        if (code <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "Success code must be a positive integer."
            );
        }

        Code = code;
        MessageProvider = message is not null ? new LiteralMessageProvider(message) : null;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Success"/> is equal to the current <see cref="Success"/>.
    /// </summary>
    /// <param name="other">The <see cref="Success"/> to compare with the current instance.</param>
    /// <returns>true if the specified <see cref="Success"/> is equal to the current <see cref="Success"/>; otherwise, false.</returns>
    /// <remarks>
    /// Equality is based on the <see cref="Code"/> property. The localized <see cref="Message"/> is not considered.
    /// </remarks>
    public virtual bool Equals(Success? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Code == other.Code;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Success other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this <see cref="Success"/>.
    /// </summary>
    /// <remarks>
    /// The hash code is based on the <see cref="Code"/> property. The localized <see cref="Message"/> is not considered.
    /// </remarks>
    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    /// <summary>
    /// Checks if two <see cref="Success"/> instances are equal.
    /// </summary>
    public static bool operator ==(Success? left, Success? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="Success"/> instances are different.
    /// </summary>
    public static bool operator !=(Success? left, Success? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a success instance with the generic code for "OK".
    /// </summary>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success"/> instance.</returns>
    public static Success Ok(string? message = null) =>
        new Successes.OkSuccess(SuccessMessageFactory.CreateProvider(message, "Success_Ok"));

    /// <summary>
    /// Creates a success instance with the generic code for "Created".
    /// </summary>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success"/> instance.</returns>
    public static Success Created(string? message = null) =>
        new Successes.CreatedSuccess(
            SuccessMessageFactory.CreateProvider(message, "Success_Created")
        );

    /// <summary>
    /// Creates a success instance with the generic code for "Accepted".
    /// </summary>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success"/> instance.</returns>
    public static Success Accepted(string? message = null) =>
        new Successes.AcceptedSuccess(
            SuccessMessageFactory.CreateProvider(message, "Success_Accepted")
        );

    /// <summary>
    /// Creates a success instance with the generic code for "No Content".
    /// </summary>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success"/> instance.</returns>
    public static Success NoContent(string? message = null) =>
        new Successes.NoContentSuccess(
            SuccessMessageFactory.CreateProvider(message, "Success_NoContent")
        );

    /// <summary>
    /// Creates a success instance with the generic code for "OK" and encapsulates a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the success.</typeparam>
    /// <param name="value">The value to be encapsulated in the success.</param>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success{TValue}"/> instance.</returns>
    public static Success<TValue> Ok<TValue>(TValue value, string? message = null) =>
        new Successes.OkSuccess<TValue>(
            value,
            SuccessMessageFactory.CreateProvider(message, "Success_Ok")
        );

    /// <summary>
    /// Creates a success instance with the generic code for "Created" and encapsulates a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the success.</typeparam>
    /// <param name="value">The value to be encapsulated in the success.</param>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success{TValue}"/> instance.</returns>
    public static Success<TValue> Created<TValue>(TValue value, string? message = null) =>
        new Successes.CreatedSuccess<TValue>(
            value,
            SuccessMessageFactory.CreateProvider(message, "Success_Created")
        );

    /// <summary>
    /// Creates a success instance with the generic code for "Accepted" and encapsulates a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the success.</typeparam>
    /// <param name="value">The value to be encapsulated in the success.</param>
    /// <param name="message">The optional success message. If not provided, the default localized message will be used.</param>
    /// <returns>A new <see cref="Success{TValue}"/> instance.</returns>
    public static Success<TValue> Accepted<TValue>(TValue value, string? message = null) =>
        new Successes.AcceptedSuccess<TValue>(
            value,
            SuccessMessageFactory.CreateProvider(message, "Success_Accepted")
        );

    /// <summary>
    /// Creates a new typed success (<see cref="Success{TValue}"/>) based on the metadata
    /// of this success (code and message), adding a typed value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the new success.</typeparam>
    /// <param name="value">The value to be encapsulated.</param>
    /// <returns>A new <see cref="Success{TValue}"/> instance with the same metadata.</returns>
    public abstract Success<TValue> WithValue<TValue>(TValue value);

    /// <summary>
    /// Represents a fluent hook used to attach domain-specific result extensions.
    /// </summary>
    /// <remarks>
    /// This class is instantiated via <see cref="Result.Of"/> and serves as the
    /// target for extension methods to avoid polluting the core Result API.
    /// </remarks>
    public sealed class Hook
    {
        internal Hook() { }
    }
}

/// <summary>
/// Represents a success result that encapsulates a specific value along with success metadata.
/// This class extends <see cref="Success"/> to provide a typed value.
/// </summary>
/// <typeparam name="TValue">The type of the success value that this object encapsulates.</typeparam>
public abstract class Success<TValue> : Success, IEquatable<Success<TValue>>
{
    /// <summary>
    /// Gets the encapsulated success value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Provides a fluent entry point to create custom success
    /// through the <see cref="Success.Hook"/> mechanism.
    /// </summary>
    /// <value>
    /// A new instance of the <see cref="Success.Hook"/> class.
    /// </value>
    public static new Hook Of => new();

    /// <remarks>
    /// Protected constructor to initialize the <see cref="Success{TValue}"/> instance.
    /// </remarks>
    /// <param name="code">The numeric code of the success.</param>
    /// <param name="messageProvider">The message provider for this success.</param>
    /// <param name="value">The success value to be encapsulated.</param>
    protected Success(int code, IMessageProvider? messageProvider, TValue value)
        : base(code, messageProvider)
    {
        Value = value;
    }

    /// <remarks>
    /// Protected constructor to initialize the <see cref="Success{TValue}"/> instance
    /// from a non-generic <see cref="Success"/>.
    /// </remarks>
    /// <param name="existingSuccess">The existing <see cref="Success"/> object.</param>
    /// <param name="value">The success value to be encapsulated.</param>
    protected Success(Success existingSuccess, TValue value)
        : base(existingSuccess.Code, existingSuccess.MessageProvider)
    {
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(Success<TValue>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Success<TValue> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + base.GetHashCode();
            hash = hash * 23 + (Value?.GetHashCode() ?? 0);
            return hash;
        }
#else
        return HashCode.Combine(base.GetHashCode(), Value);
#endif
    }

    /// <summary>
    /// Checks if two <see cref="Success{TValue}"/> instances are equal.
    /// </summary>
    public static bool operator ==(Success<TValue>? left, Success<TValue>? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="Success{TValue}"/> instances are different.
    /// </summary>
    public static bool operator !=(Success<TValue>? left, Success<TValue>? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Represents a fluent hook used to attach domain-specific result extensions.
    /// </summary>
    /// <remarks>
    /// This class is instantiated via <see cref="Result.Of"/> and serves as the
    /// target for extension methods to avoid polluting the core Result API.
    /// </remarks>
    public new sealed class Hook
    {
        internal Hook() { }
    }
}
