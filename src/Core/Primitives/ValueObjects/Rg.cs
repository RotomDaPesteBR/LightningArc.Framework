using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a Brazilian RG (Registro Geral) — the national identity document number.
    /// </summary>
    public record Rg : IValueObject<string>
    {
        private const string RgRegexPattern = @"^\d{1,2}\.?\d{3}\.?\d{3}-?[0-9Xx]?$";

        /// <summary>
        /// Gets the string value of the RG.
        /// </summary>
        public string Value { get; }

        internal Rg(string value)
        {
            if (!IsValid(value, out string? errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            Value = Clean(value);
        }

        /// <summary>
        /// Creates a new <see cref="Rg"/> from the specified string value.
        /// </summary>
        public static Rg Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new <see cref="Rg"/> from the specified string value.
        /// </summary>
        public static bool TryCreate(string value, [NotNullWhen(true)] out Rg? result)
        {
            if (IsValid(value, out _))
            {
                result = new Rg(value);
                return true;
            }

            result = null;
            return false;
        }

        internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = "The RG cannot be null or empty.";
                return false;
            }

            string cleanRg = Clean(value);

            if (cleanRg.Length < 7 || cleanRg.Length > 11 || !Regex.IsMatch(cleanRg, RgRegexPattern, RegexOptions.None, TimeSpan.FromMilliseconds(50)))
            {
                errorMessage = $"The value '{value}' is not a valid RG.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static string Clean(string value) => new([.. value.Where(c => char.IsDigit(c) || c == 'X' || c == 'x')]);

        /// <summary>
        /// Implicitly converts an <see cref="Rg"/> to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Rg rg) => rg?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to an <see cref="Rg"/> object, triggering validation.
        /// </summary>
        public static implicit operator Rg(string value) => Create(value);

        /// <inheritdoc/>
        public override string ToString() => Value;
    }
}
