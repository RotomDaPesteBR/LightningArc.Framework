using System.Globalization;
using LightningArc.Results.Messages;
using static LightningArc.Results.Business;

namespace LightningArc.Results;

public class SuccessMessageProvider : IMessageProvider
{
    /// <inheritdoc />
    public string GetMessage(CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderProcessedSuccess : Success
{
    /// <inheritdoc />
    internal OrderProcessedSuccess(IMessageProvider? messageProvider)
        : base(201, messageProvider) { }

    /// <inheritdoc />
    internal OrderProcessedSuccess(string? message)
        : base(201, message) { }

    public override Success<TValue> WithValue<TValue>(TValue value)
    {
        throw new NotImplementedException();
    }
}

public class OrderProcessedSuccess<TValue> : Success<TValue>
{
    /// <inheritdoc />
    internal OrderProcessedSuccess(TValue value, IMessageProvider? messageProvider)
        : base(201, messageProvider, value) { }

    /// <inheritdoc />
    internal OrderProcessedSuccess(TValue value, string? message)
        : base(201, new LiteralMessageProvider(message ?? "Order processed"), value) { }

    public override Success<TMappedValue> WithValue<TMappedValue>(TMappedValue value)
    {
        throw new NotImplementedException();
    }
}

public static partial class BusinessSuccessExtensions
{
    public static Result OrderProcessed(this Success.Hook _, string? message = null)
    {
        return new OrderProcessedSuccess(message);
    }

    //public static Result<TValue> OrderProcessed<TValue>(
    //    this Success<TValue>.Hook _,
    //    TValue value,
    //    string? message = null
    //)
    //{
    //    return new OrderProcessedSuccess<TValue>(value, message);
    //}

    extension<TValue>(Success<TValue>.Hook _)
    {
        public Result<TValue> OrderProcessed(TValue value, string? message = null)
        {
            return new OrderProcessedSuccess<TValue>(value, message);
        }
    }

    /*public static Result<TValue> OrderProcessed(
        this Success<TValue>.Hook _,
        TValue value,
        string? message = null
    )
    {
        return new OrderProcessedSuccess<TValue>(value, message);
    }*/
}

public static partial class ResultExtensions
{
    extension(Result)
    {
        public static Result OrderProcessed(string? message = null)
        {
            return new OrderProcessedSuccess(message);
        }
    }

    extension<TValue>(Result)
    {
        public static Result<TValue> OrderProcessed(TValue value, string? message = null)
        {
            return new OrderProcessedSuccess<TValue>(value, message);
        }
    }
}

public class Business : Error.ErrorModule
{
    public new const int CodePrefix = 12;

    // Specific error class for the business module
    public class OrderRejectedError : Error
    {
        internal OrderRejectedError(string message, params IEnumerable<ErrorDetail>? details)
            : base(Business.CodePrefix, 01, message, details) { }
    }
}

public static class BusinessErrorExtensions
{
    extension(Error)
    {
        public static Error.ErrorModule<Business> Business => Error.Of<Business>();
    }

    public static Error OrderRejected(
        this Error.ErrorModule<Business> _,
        string message = "Pedido rejeitado",
        params IEnumerable<ErrorDetail>? details
    ) => new OrderRejectedError(message, details);
}
