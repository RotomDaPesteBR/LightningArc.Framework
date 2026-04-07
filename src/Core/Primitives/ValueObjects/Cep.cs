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
public record Cep : IValueObject<string>
{
    private const string CepRegexPattern = @"^\d{5}-?\d{3}$";

    /// <summary>
    /// Gets the string value of the CEP.
    /// </summary>
    public string Value { get; }

    private Cep(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The CEP cannot be null or empty.", nameof(value));
        }

        var normalized = Normalize(value);

        if (!Regex.IsMatch(normalized, CepRegexPattern, RegexOptions.None, TimeSpan.FromMilliseconds(50)))
        {
            throw new ArgumentException($"The value '{value}' is not a valid CEP.", nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Cep"/> after validation.
    /// </summary>
    public static Cep Create(string value) => new(value);

    /// <summary>
    /// Tries to create a new instance of <see cref="Cep"/>.
    /// </summary>
    public static bool TryCreate(string value, out Cep? result)
    {
        try
        {
            result = new Cep(value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null;
            return false;
        }
    }

    private static string Normalize(string value)
    {
        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 8)
        {
            return $"{digitsOnly.Substring(0, 5)}-{digitsOnly.Substring(5)}";
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
