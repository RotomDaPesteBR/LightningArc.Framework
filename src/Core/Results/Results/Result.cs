namespace LightningArc.Results;

/// <summary>
/// Represents the result of an operation that can be either a success or a failure.
/// This is the base class for the Result pattern, used when an operation does not return a specific value upon success.
/// </summary>
/// <remarks>
/// A <see cref="Result"/> is immutable, and its nature (success or failure) is defined at the time of creation.
/// In case of failure, it encapsulates an <see cref="Error"/> object that provides details about what happened.
/// </remarks>
public class Result : IEquatable<Result>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the generic status code associated with the result.
    /// </summary>
    /// <remarks>
    /// This is a convenience value that returns the code from the <see cref="Results.Success"/> object on success
    /// or from the <see cref="Error"/> object on failure.
    /// </remarks>
    public int Code => IsSuccess ? SuccessDetails.Code : Error.Code;

    /// <summary>
    /// Gets the message associated with the result.
    /// </summary>
    /// <remarks>
    /// This property returns the message from the <see cref="Results.Success"/> object if the operation was successful,
    /// or the message from the <see cref="Error"/> object if the operation failed.
    /// </remarks>
    public string? Message => IsSuccess ? SuccessDetails.Message : Error.Message;

    /// <summary>
    /// Gets the <see cref="Error"/> object associated with this result.
    /// </summary>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is successful (there is no error to access).</exception>
    public Error Error =>
        IsFailure
            ? _error!
            : throw new ResultAccessFailedException("Result is successful, no error to access.");

    /// <summary>
    /// Provides a fluent entry point to create custom success
    /// through the <see cref="Success.Hook"/> mechanism.
    /// </summary>
    /// <value>
    /// A new instance of the <see cref="Success.Hook"/> class.
    /// </value>
    public static Success.Hook Of => Results.Success.Of;

    /// <summary>
    /// Gets the <see cref="Results.Success"/> object associated with this result.
    /// </summary>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is a failure (there are no success details to access).</exception>
    public Success SuccessDetails =>
        IsSuccess
            ? _success!
            : throw new ResultAccessFailedException(
                "Result is not successful, no success details to access."
            );

    private readonly Error? _error;
    private readonly Success? _success;

    /// <summary>
    /// Protected constructor for a success result.
    /// </summary>
    /// <param name="success">The <see cref="Results.Success"/> object containing the code and message.</param>
    protected Result(Success success)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(success);
#else
        if (success is null)
        {
            throw new ArgumentNullException(nameof(success));
        }
#endif

        IsSuccess = true;
        _success = success;
        _error = null;
    }

    /// <summary>
    /// Protected constructor for a failure result.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object describing the failure.</param>
    protected Result(Error error)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(error);
#else
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }
#endif

        IsSuccess = false;
        _error = error;
        _success = null;
    }

    /// <summary>
    /// Protected copy constructor.
    /// </summary>
    /// <param name="result">The <see cref="Result"/> object to be copied.</param>
    protected Result(Result result)
    {
        IsSuccess = result.IsSuccess;
        _success = result._success;
        _error = result._error;
    }

    /// <inheritdoc />
    public bool Equals(Result? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (IsSuccess != other.IsSuccess)
        {
            return false;
        }

        return IsSuccess ? Equals(_success, other._success) : Equals(_error, other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is Result other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + IsSuccess.GetHashCode();
            if (IsSuccess)
            {
                hash = hash * 23 + (_success?.GetHashCode() ?? 0);
            }
            else
            {
                hash = hash * 23 + (_error?.GetHashCode() ?? 0);
            }
            return hash;
        }
#else
        return IsSuccess
            ? HashCode.Combine(IsSuccess, _success)
            : HashCode.Combine(IsSuccess, _error);
#endif
    }

    /// <summary>
    /// Checks if the result is successful.
    /// Allows the result object to be used directly in conditional expressions.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public static bool operator true(Result result) => result.IsSuccess;

    /// <summary>
    /// Checks if the result is a failure.
    /// Allows the result object to be used directly in conditional expressions.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
    public static bool operator false(Result result) => result.IsFailure;

    /// <summary>
    /// Negates the success status of the result.
    /// </summary>
    /// <param name="result">The result to negate.</param>
    /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
    public static bool operator !(Result result) => result.IsFailure;

    /// <summary>
    /// Deconstructs the result into its success status and error.
    /// </summary>
    /// <param name="isSuccess">Indicates if the operation was successful.</param>
    /// <param name="error">The error object if the operation failed.</param>
    public void Deconstruct(out bool isSuccess, out Error? error)
    {
        isSuccess = IsSuccess;
        error = _error;
    }

    /// <summary>
    /// Attempts to retrieve the error, returning true if the result is a failure.
    /// </summary>
    /// <param name="error">The error if the result is a failure; otherwise, <c>null</c>.</param>
    /// <returns>True if the result is a failure; otherwise, false.</returns>
    public bool TryGetError(out Error error)
    {
        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        error = null!;
        return false;
    }

    /// <summary>
    /// Checks if two <see cref="Result"/> instances are equal.
    /// </summary>
    public static bool operator ==(Result? left, Result? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="Result"/> instances are different.
    /// </summary>
    public static bool operator !=(Result? left, Result? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a success result with a generic code (Ok) and an optional message.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result Success() => new(Results.Success.Ok());

    /// <summary>
    /// Creates a success result with the specified Success details.
    /// </summary>
    /// <param name="success">The success details.</param>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result Success(Success success) => new(success);

    /// <summary>
    /// Creates a success result with a generic code (Created) and an optional message.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result Created() => new(Results.Success.Created());

    /// <summary>
    /// Creates a success result with a generic code (Accepted) and an optional message.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result Accepted() => new(Results.Success.Accepted());

    /// <summary>
    /// Creates a success result with a generic code (NoContent).
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result NoContent() => new(Results.Success.NoContent());

    /// <summary>
    /// Starts a fluent aggregation process to collect multiple results or exceptions.
    /// </summary>
    /// <returns>A new <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Aggregate() => new();

    // Factory methods that create a Result with value

    /// <summary>
    /// Transforms a non-generic <see cref="Result"/> (without a value) into a <see cref="Result{TValue}"/> (with a value).
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the new result.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/>, maintaining the success/failure status of the original result.
    /// If the original result was successful, it encapsulates the new value. If it was a failure, it returns the original failure.</returns>
    public Result<TValue> WithValue<TValue>(TValue value) => new(value, this);

    /// <summary>
    /// Creates a success result with a value and a generic code (Ok), using the default message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(Results.Success.Ok(value));

    /// <summary>
    /// Creates a success result with a value and a generic code (Ok), with a custom message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <param name="message">The custom success message.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success.</returns>
    public static Result<TValue> Success<TValue>(TValue value, string message) =>
        new(Results.Success.Ok(value, message));

    /// <summary>
    /// Creates a success result with a value and a generic code (Ok), with a custom message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <param name="success">The success details.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success.</returns>
    public static Result<TValue> Success<TValue>(TValue value, Success success) =>
        new(success.WithValue(value));

    /// <summary>
    /// Creates a success result with the specified Success details.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="success">The success details.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success.</returns>
    public static Result<TValue> Success<TValue>(Success<TValue> success) => new(success);

    // --- Created ---

    /// <summary>
    /// Creates a success result with a value and a generic code (Created), using the default message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and creation.</returns>
    public static Result<TValue> Created<TValue>(TValue value) =>
        new(Results.Success.Created(value));

    /// <summary>
    /// Creates a success result with a value and a generic code (Created), with a custom message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <param name="message">The custom success message.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and creation.</returns>
    public static Result<TValue> Created<TValue>(TValue value, string message) =>
        new(Results.Success.Created(value, message));

    // --- Accepted ---

    /// <summary>
    /// Creates a success result with a value and a generic code (Accepted), using the default message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and acceptance.</returns>
    public static Result<TValue> Accepted<TValue>(TValue value) =>
        new(Results.Success.Accepted(value));

    /// <summary>
    /// Creates a success result with a value and a generic code (Accepted), with a custom message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <param name="message">The custom success message.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and acceptance.</returns>
    public static Result<TValue> Accepted<TValue>(TValue value, string message) =>
        new(Results.Success.Accepted(value, message));

    // --- NoContent ---

    /// <summary>
    /// Creates a success result with a value and a generic code (No Content).
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and no content.</returns>
    public static Result<TValue> NoContent<TValue>(TValue value) =>
        new(Results.Success.NoContent(value));

    /// <summary>
    /// Creates a success result with a value and a generic code (No Content).
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <param name="message">The custom success message.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating success and no content.</returns>
    public static Result<TValue> NoContent<TValue>(TValue value, string message) =>
        new(Results.Success.NoContent(value, message));

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object describing the failure.</param>
    /// <returns>A new instance of <see cref="Result"/> indicating failure.</returns>
    public static Result Failure(Error error) => new(error);

    /// <summary>
    /// Allows implicit conversion from an <see cref="Error"/> to a failure <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object to be converted.</param>
    /// <returns>A failure <see cref="Result"/> encapsulating the error.</returns>
    public static implicit operator Result(Error error) => new(error);

    /// <summary>
    /// Allows implicit conversion from a <see cref="Results.Success"/> to a success <see cref="Result"/>.
    /// </summary>
    /// <param name="success">The <see cref="Results.Success"/> object to be converted.</param>
    /// <returns>A success <see cref="Result"/> encapsulating the success.</returns>
    public static implicit operator Result(Success success) => new(success);
}

/// <summary>
/// Represents the result of an operation that can be a success (with a specific value) or a failure.
/// </summary>
/// <typeparam name="TValue">The type of the success value that this result encapsulates.</typeparam>
/// <remarks>
/// This class inherits from <see cref="Result"/> and adds the ability to carry a success value.
/// It is the preferred way to return results from operations that produce data.
/// </remarks>
public class Result<TValue> : Result, IEquatable<Result<TValue>>
{
    /// <summary>
    /// Gets the <see cref="Success"/> object associated with this result.
    /// </summary>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is a failure (there are no success details to access).</exception>
    public new Success<TValue> SuccessDetails =>
        IsSuccess
            ? _success!
            : throw new ResultAccessFailedException(
                "Result is not successful, no success details to access."
            );

    /// <summary>
    /// Gets the success value encapsulated by this result.
    /// </summary>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is not successful (there is no value to access).</exception>
    public TValue Value =>
        IsSuccess
            ? _success!.Value
            : throw new ResultAccessFailedException("Result is not successful.");
    private readonly Success<TValue>? _success;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a success value.
    /// </summary>
    /// <param name="value">The success value to be encapsulated.</param>
    /// <param name="success">The <see cref="Success"/> object containing the code and message.</param>
    internal Result(TValue value, Success success)
        : base(success)
    {
        _success = success.WithValue(value);
    }

    /// <summary>
    /// Protected constructor for a success result.
    /// </summary>
    /// <param name="success">The <see cref="Success"/> object containing the code and message.</param>
    internal Result(Success<TValue> success)
        : base(success)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(success);
#else
        if (success is null)
        {
            throw new ArgumentNullException(nameof(success));
        }
#endif

        _success = success;
    }

    /// <summary>
    /// Internal constructor used to attach a value to an existing result.
    /// </summary>
    /// <param name="value">The value to be encapsulated.</param>
    /// <param name="result">The existing non-generic (<see cref="Result"/>) result.</param>
    internal Result(TValue value, Result result)
        : base(result)
    {
        if (result.IsSuccess)
        {
            _success = result.SuccessDetails.WithValue(value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a failure error.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object describing the failure.</param>
    internal Result(Error error)
        : base(error) { }

    /// <inheritdoc />
    public bool Equals(Result<TValue>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!base.Equals(other))
        {
            return false;
        }

        if (IsSuccess)
        {
            return Equals(_success, other._success);
        }

        return true; // Se ambos falharam e o base.Equals passou, os erros são iguais
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is Result<TValue> other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Attempts to retrieve the success value, returning true if the result is successful.
    /// </summary>
    /// <param name="value">The success value if the result is successful; otherwise, <c>default</c>.</param>
    /// <returns>True if the result is successful; otherwise, false.</returns>
    public bool TryGetValue(out TValue value)
    {
        if (IsSuccess)
        {
            value = _success!.Value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Deconstructs the result into its success status, value, and error.
    /// </summary>
    /// <param name="isSuccess">Indicates if the operation was successful.</param>
    /// <param name="value">The success value if the operation was successful.</param>
    /// <param name="error">The error object if the operation failed.</param>
    public void Deconstruct(out bool isSuccess, out TValue? value, out Error? error)
    {
        isSuccess = IsSuccess;
        value = IsSuccess ? Value : default;
        error = IsFailure ? Error : null;
    }

    /// <summary>
    /// Checks if two <see cref="Result{TValue}"/> instances are equal.
    /// </summary>
    public static bool operator ==(Result<TValue>? left, Result<TValue>? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="Result{TValue}"/> instances are different.
    /// </summary>
    public static bool operator !=(Result<TValue>? left, Result<TValue>? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a generic failure result.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object describing the failure.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> indicating failure.</returns>
    public static new Result<TValue> Failure(Error error) => new(error);

    /// <summary>
    /// Allows the conversion of a generic result to a non-generic result.
    /// </summary>
    /// <param name="result">The value to be converted.</param>
    /// <returns>A success <see cref="Result{TValue}"/> encapsulating the value.</returns>
    public static Result ToResult(Result<TValue> result) =>
        result.IsSuccess ? Result.Success(result.SuccessDetails) : Result.Failure(result.Error);

    /// <summary>
    /// Explicitly converts a <see cref="Result{TValue}"/> to its encapsulated value.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is a failure.</exception>
    public static explicit operator TValue(Result<TValue> result) => result.Value;

    /// <summary>
    /// Explicitly converts a <see cref="Result{TValue}"/> to its encapsulated error.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <exception cref="ResultAccessFailedException">Thrown if the result is a success.</exception>
    public static explicit operator Error(Result<TValue> result) => result.Error;

    /// <summary>
    /// Allows implicit conversion from a value to a success <see cref="Result{TValue}"/>.
    /// The success result will have the generic code "Ok" (100).
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <returns>A success <see cref="Result{TValue}"/> encapsulating the value.</returns>
    public static implicit operator Result<TValue>(TValue value) =>
        new(value, Results.Success.Ok());

    /// <summary>
    /// Allows implicit conversion from an <see cref="Error"/> to a failure <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> object to be converted.</param>
    /// <returns>A failure <see cref="Result{TValue}"/> encapsulating the error.</returns>
    public static implicit operator Result<TValue>(Error error) => new(error);

    /// <summary>
    /// Allows implicit conversion from a <see cref="Success{TValue}"/> to a success <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="success">The <see cref="Success{TValue}"/> object to be converted.</param>
    /// <returns>A success <see cref="Result{TValue}"/>.</returns>
    public static implicit operator Result<TValue>(Success<TValue> success) => new(success);
}
