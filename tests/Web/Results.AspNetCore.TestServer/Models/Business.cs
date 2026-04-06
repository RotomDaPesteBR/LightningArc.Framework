using static LightningArc.Results.Business;

namespace LightningArc.Results;

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
    public static Error OrderRejected(
        this Error.ErrorModule<Business> _,
        string message = "Pedido rejeitado",
        List<ErrorDetail>? details = null
    ) => new OrderRejectedError(message, details);
}

