using LightningArc.Results.Messages;
using static LightningArc.Results.Business;

namespace LightningArc.Results;

public class OrderProcessedSuccess : Success
{
    /// <inheritdoc />
    internal OrderProcessedSuccess(int code, IMessageProvider? messageProvider) : base(code, messageProvider)
    {
    }

    /// <inheritdoc />
    internal OrderProcessedSuccess(int code, string? message) : base(code, message)
    {
    }

    public override Success<TValue> WithValue<TValue>(TValue value)
    {
        throw new NotImplementedException();
    }
}

public static partial class BusinessSuccessExtensions
{
    public static Result OrderProcessed(this Success.Hook _)
    {
        return new OrderProcessedSuccess(103, "Custom success");
    }
}

public static partial class ResultExtensions
{
    extension(Result)
    {
        public static Result OrderProcessed()
        {
            return new OrderProcessedSuccess(103, "Custom success");
        }
    }
}

public class Business : Error.ErrorModule
{
    public new const int CodePrefix = 12;

    // Classe de erro específica do módulo de negócio
    public class OrderRejectedError : Error
    {
        internal OrderRejectedError(string message, List<ErrorDetail>? details = null)
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
        List<ErrorDetail>? details = null
    ) => new OrderRejectedError(message, details);
}

