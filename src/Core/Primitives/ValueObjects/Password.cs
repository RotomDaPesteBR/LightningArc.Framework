using System.Diagnostics.CodeAnalysis;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Represents a validated password with configurable strength policy.
/// </summary>
public record Password : IValueObject<string>
{
    /// <summary>
    /// Gets the raw password value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the strength level of the password.
    /// </summary>
    public PasswordStrength Strength { get; }

    internal Password(string value, PasswordStrength minimumStrength)
    {
        if (!IsValid(value, minimumStrength, out string? errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(value));
        }

        Value = value;
        Strength = CalculateStrength(value);
    }

    /// <summary>
    /// Creates a new <see cref="Password"/> from the specified string value.
    /// </summary>
    public static Password Create(string value, PasswordStrength minimumStrength = PasswordStrength.Moderate) => new(value, minimumStrength);

    /// <summary>
    /// Tries to create a new <see cref="Password"/> from the specified string value.
    /// </summary>
    public static bool TryCreate(string value, [NotNullWhen(true)] out Password? result, PasswordStrength minimumStrength = PasswordStrength.Moderate)
    {
        if (IsValid(value, minimumStrength, out _))
        {
            result = new Password(value, minimumStrength);
            return true;
        }

        result = null;
        return false;
    }

    internal static bool IsValid(string value, PasswordStrength minimumStrength, [NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "The password cannot be null or empty.";
            return false;
        }

        if (value.Length < 8)
        {
            errorMessage = "The password must have at least 8 characters.";
            return false;
        }

        if (!value.Any(char.IsDigit))
        {
            errorMessage = "The password must contain at least one digit.";
            return false;
        }

        if (!value.Any(char.IsUpper))
        {
            errorMessage = "The password must contain at least one uppercase letter.";
            return false;
        }

        if (!value.Any(char.IsLower))
        {
            errorMessage = "The password must contain at least one lowercase letter.";
            return false;
        }

        PasswordStrength strength = CalculateStrength(value);

        if ((int)strength < (int)minimumStrength)
        {
            errorMessage = $"The password strength '{strength}' does not meet the minimum requirement '{minimumStrength}'.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static PasswordStrength CalculateStrength(string value)
    {
        int score = 0;
        if (value.Length >= 10)
        {
            score++;
        }

        if (value.Any(char.IsSymbol))
        {
            score++;
        }

        if (value.Length >= 14)
        {
            score++;
        }

        if (value.Length >= 18)
        {
            score++;
        }

        if (score >= 4)
        {
            return PasswordStrength.Strong;
        }

        return score switch
        {
            >= 2 => PasswordStrength.Moderate,
            _ => PasswordStrength.Weak
        };
    }

    /// <summary>
    /// Implicitly converts a <see cref="Password"/> to its <see cref="string"/> representation.
    /// </summary>
    public static implicit operator string(Password password) => password?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

    /// <summary>
    /// Returns a masked string to prevent accidental password exposure.
    /// </summary>
    public override string ToString() => new('*', Value.Length);
}
