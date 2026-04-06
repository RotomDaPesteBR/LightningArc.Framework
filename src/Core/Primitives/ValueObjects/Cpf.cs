using System.Linq;
using LightningArc.Primitives;

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
    public record Cpf : IValueObject<string>
    {
        /// <summary>
        /// Gets the string value of the CPF.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Private constructor for the Cpf class.
        /// This constructor performs the validation of the CPF string.
        /// </summary>
        /// <param name="value">The string representing the CPF.</param>
        /// <exception cref="ArgumentException">Thrown if 'value' is null, empty, or not a valid CPF.</exception>
        private Cpf(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("O CPF não pode ser nulo ou vazio.", nameof(value));
            }

            var cleanCpf = new string(value.Where(char.IsDigit).ToArray());

            if (cleanCpf.Length != 11 || IsRepeatedDigits(cleanCpf))
            {
                throw new ArgumentException($"O valor '{value}' não é um CPF válido.", nameof(value));
            }

            if (!IsValidCpf(cleanCpf))
            {
                throw new ArgumentException($"O valor '{value}' não é um CPF válido.", nameof(value));
            }

            Value = cleanCpf;
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
        public static bool TryCreate(string value, out Cpf? result)
        {
            try
            {
                result = new Cpf(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        private static bool IsRepeatedDigits(string value) => value.Distinct().Count() == 1;

        private static bool IsValidCpf(string cpf)
        {
            var tempCpf = cpf.Substring(0, 9);
            var sum = 0;

            for (var i = 0; i < 9; i++)
            {
                sum += (tempCpf[i] - '0') * (10 - i);
            }

            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            tempCpf += digit1;
            sum = 0;

            for (var i = 0; i < 10; i++)
            {
                sum += (tempCpf[i] - '0') * (11 - i);
            }

            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return cpf.EndsWith(digit1.ToString() + digit2.ToString());
        }

        /// <summary>
        /// Implicitly converts a <see cref="Cpf"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Cpf cpf) => cpf?.Value ?? throw new InvalidOperationException("Não é possível converter um ValueObject nulo para string.");

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
