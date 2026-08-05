using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Represents a monetary value with an ISO 4217 currency code.
/// </summary>
public sealed record Currency : IValueObject<decimal>
{
    private static readonly Dictionary<string, int> KnownCodes = new()
    {
        { "BRL", 2 }, { "USD", 2 }, { "EUR", 2 }, { "GBP", 2 }, { "JPY", 0 },
        { "CHF", 2 }, { "CAD", 2 }, { "AUD", 2 }, { "NZD", 2 }, { "CNY", 2 },
        { "ARS", 2 }, { "CLP", 0 }, { "COP", 2 }, { "MXN", 2 }, { "PEN", 2 },
        { "UYU", 2 }, { "PYG", 0 }, { "BOB", 2 }, { "VES", 2 }, { "CRC", 2 },
        { "INR", 2 }, { "RUB", 2 }, { "ZAR", 2 }, { "KRW", 0 }, { "SGD", 2 },
        { "HKD", 2 }, { "THB", 2 }, { "TWD", 2 }, { "SEK", 2 }, { "NOK", 2 },
        { "DKK", 2 }, { "PLN", 2 }, { "CZK", 2 }, { "HUF", 2 }, { "RON", 2 },
        { "BGN", 2 }, { "TRY", 2 }, { "ILS", 2 }, { "AED", 2 }, { "SAR", 2 },
        { "QAR", 2 }, { "KWD", 3 }, { "BHD", 3 }, { "OMR", 3 }, { "JOD", 3 },
        { "EGP", 2 }, { "NGN", 2 }, { "KES", 2 }, { "GHS", 2 }, { "ETB", 2 }
    };

    /// <summary>
    /// Gets the monetary value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the ISO 4217 currency code (e.g., "BRL", "USD").
    /// </summary>
    public string Code { get; }

    internal Currency(decimal value, string code)
    {
        if (!IsValid(value, code, out string? errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(code));
        }

        Value = value;
        Code = code.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new instance of <see cref="Currency"/> after validation.
    /// </summary>
    public static Currency Create(decimal value, string code) => new(value, code);

    /// <summary>
    /// Tries to create a new instance of <see cref="Currency"/>.
    /// </summary>
    public static bool TryCreate(decimal value, string code, [NotNullWhen(true)] out Currency? result)
    {
        if (IsValid(value, code, out _))
        {
            result = new Currency(value, code);
            return true;
        }

        result = null;
        return false;
    }

    internal static bool IsValid(decimal value, string code, [NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            errorMessage = "The currency code cannot be null or empty.";
            return false;
        }

        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != 3 || !normalizedCode.All(c => c >= 'A' && c <= 'Z'))
        {
            errorMessage = $"The currency code '{code}' must be a valid 3-letter ISO 4217 code.";
            return false;
        }

        if (!KnownCodes.TryGetValue(normalizedCode, out int precision))
        {
            errorMessage = $"The currency code '{code}' is not a valid ISO 4217 code.";
            return false;
        }

        // Validate decimal precision (minor units)
        if (decimal.Round(value, precision) != value)
        {
            errorMessage = $"The value '{value}' has too many decimal places for currency '{normalizedCode}'. Expected at most {precision} decimal places.";
            return false;
        }

        errorMessage = null;
        return true;
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
