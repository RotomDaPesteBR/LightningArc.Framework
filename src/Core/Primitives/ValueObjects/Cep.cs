using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Represents a Brazilian CEP (Cadastro de Endereços Postais).
/// </summary>
/// <remarks>
/// This 'record' is an immutable value type. CEP format validation (XXXXX-XXX or 8 digits)
/// is enforced in the private constructor.
/// </remarks>
public sealed record Cep : IValueObject<string>
{
    private const string CepRegexPattern = @"^\d{5}-?\d{3}$";

    /// <summary>
    /// Gets the string value of the CEP.
    /// </summary>
    public string Value { get; }

    internal Cep(string value)
    {
        if (!IsValid(value, out string? errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(value));
        }

        Value = Normalize(value);
    }

    /// <summary>
    /// Creates a new instance of <see cref="Cep"/> after validation.
    /// </summary>
    public static Cep Create(string value) => new(value);

    /// <summary>
    /// Tries to create a new instance of <see cref="Cep"/>.
    /// </summary>
    public static bool TryCreate(string value, [NotNullWhen(true)] out Cep? result)
    {
        if (IsValid(value, out _))
        {
            result = new Cep(value);
            return true;
        }

        result = null;
        return false;
    }

    internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "The CEP cannot be null or empty.";
            return false;
        }

        string normalized = Normalize(value);

        if (!Regex.IsMatch(normalized, CepRegexPattern, RegexOptions.None, TimeSpan.FromMilliseconds(50)))
        {
            errorMessage = $"The value '{value}' is not a valid CEP.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static string Normalize(string value)
    {
        string digitsOnly = new([.. value.Where(char.IsDigit)]);
        if (digitsOnly.Length == 8)
        {
            return $"{digitsOnly[..5]}-{digitsOnly[5..]}";
        }
        return value.Trim();
    }

    /// <summary>
    /// Implicitly converts a <see cref="Cep"/> to its <see cref="string"/> representation.
    /// </summary>
    public static implicit operator string(Cep cep) => cep?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Cep"/>.
    /// </summary>
    public static implicit operator Cep(string value) => Create(value);

    /// <summary>
    /// Returns the string representation of the CEP.
    /// </summary>
    public override string ToString() => Value;
}
