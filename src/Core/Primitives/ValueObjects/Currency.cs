using System.Globalization;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Represents a monetary value with an ISO 4217 currency code.
/// </summary>
public record Currency : IValueObject<decimal>
{
    private static readonly HashSet<string> KnownCodes =
    [
        "BRL", "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "NZD", "CNY",
        "ARS", "CLP", "COP", "MXN", "PEN", "UYU", "PYG", "BOB", "VES", "CRC",
        "INR", "RUB", "ZAR", "KRW", "SGD", "HKD", "THB", "TWD", "SEK", "NOK",
        "DKK", "PLN", "CZK", "HUF", "RON", "BGN", "TRY", "ILS", "AED", "SAR",
        "QAR", "KWD", "BHD", "OMR", "JOD", "EGP", "NGN", "KES", "GHS", "ETB",
    ];

    /// <summary>
    /// Gets the monetary value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the ISO 4217 currency code (e.g., "BRL", "USD").
    /// </summary>
    public string Code { get; }

    private Currency(decimal value, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("The currency code cannot be null or empty.", nameof(code));
        }

        code = code.Trim().ToUpperInvariant();

        if (code.Length != 3 || !code.All(c => c >= 'A' && c <= 'Z'))
        {
            throw new ArgumentException($"The currency code '{code}' must be a valid 3-letter ISO 4217 code.", nameof(code));
        }

        if (!KnownCodes.Contains(code))
        {
            throw new ArgumentException($"The currency code '{code}' is not a valid ISO 4217 code.", nameof(code));
        }

        Value = value;
        Code = code;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Currency"/> after validation.
    /// </summary>
    public static Currency Create(decimal value, string code) => new(value, code);

    /// <summary>
    /// Tries to create a new instance of <see cref="Currency"/>.
    /// </summary>
    public static bool TryCreate(decimal value, string code, out Currency? result)
    {
        try
        {
            result = new Currency(value, code);
            return true;
        }
        catch (ArgumentException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="Currency"/> to its <see cref="decimal"/> value.
    /// </summary>
    public static implicit operator decimal(Currency currency) => currency?.Value ?? 0m;

    /// <summary>
    /// Returns the string representation (e.g., "1234.56 BRL").
    /// </summary>
    public override string ToString() => $"{Value} {Code}";
}
