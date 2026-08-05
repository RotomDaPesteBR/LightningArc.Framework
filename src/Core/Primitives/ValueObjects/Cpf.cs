using LightningArc.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a data type for a Brazilian CPF (Cadastro de Pessoas Físicas),
    /// ensuring its validity at the time of creation through a factory method.
    /// </summary>
    /// <remarks>
    /// This 'record' is an immutable value type. CPF validation is enforced
    /// in the private constructor, accessible through the <see cref="Create(string)"/> static factory method.
    /// </remarks>
    public sealed record Cpf : IValueObject<string>
    {
        /// <summary>
        /// Gets the string value of the CPF.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Internal constructor for the Cpf class.
        /// This constructor performs the validation of the CPF string.
        /// </summary>
        /// <param name="value">The string representing the CPF.</param>
        /// <exception cref="ArgumentException">Thrown if 'value' is null, empty, or not a valid CPF.</exception>
        internal Cpf(string value)
        {
            if (!IsValid(value, out string? errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            Value = new string([.. value.Where(char.IsDigit)]);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Cpf"/> after validating the CPF format.
        /// </summary>
        /// <param name="value">The string representing the CPF.</param>
        /// <returns>A new instance of <see cref="Cpf"/> if the string is a valid CPF.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided string is not a valid CPF.</exception>
        public static Cpf Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new instance of <see cref="Cpf"/> from the provided string.
        /// </summary>
        /// <param name="value">The string representing the CPF.</param>
        /// <param name="result">The resulting <see cref="Cpf"/> object, or null if validation fails.</param>
        /// <returns>True if the CPF was successfully created; otherwise, false.</returns>
        public static bool TryCreate(string value, [NotNullWhen(true)] out Cpf? result)
        {
            if (IsValid(value, out _))
            {
                result = new Cpf(value);
                return true;
            }

            result = null;
            return false;
        }

        internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = "The CPF cannot be null or empty.";
                return false;
            }

            string cleanCpf = new([.. value.Where(char.IsDigit)]);

            if (cleanCpf.Length != 11 || IsRepeatedDigits(cleanCpf) || !IsValidCpf(cleanCpf))
            {
                errorMessage = $"The value '{value}' is not a valid CPF.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool IsRepeatedDigits(string value) => value.Distinct().Count() == 1;

        private static bool IsValidCpf(string cpf)
        {
            string tempCpf = cpf[..9];
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += (tempCpf[i] - '0') * (10 - i);
            }

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            tempCpf += digit1;
            sum = 0;

            for (int i = 0; i < 10; i++)
            {
                sum += (tempCpf[i] - '0') * (11 - i);
            }

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return cpf.EndsWith(digit1.ToString() + digit2.ToString());
        }

        /// <summary>
        /// Implicitly converts a <see cref="Cpf"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Cpf cpf) => cpf?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to a <see cref="Cpf"/> object.
        /// </summary>
        public static implicit operator Cpf(string value) => Create(value);

        /// <summary>
        /// Returns the string representation of the CPF.
        /// </summary>
        public override string ToString() => Value;
    }
}
