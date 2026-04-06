using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a valid Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica).
    /// </summary>
    public record Cnpj : IValueObject<string>
    {
        /// <summary>
        /// Gets the numeric string value of the CNPJ.
        /// </summary>
        public string Value { get; }

        private Cnpj(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("O CNPJ não pode ser nulo ou vazio.", nameof(value));
            }

            var numericCnpj = new string(value.Where(char.IsDigit).ToArray());

            if (numericCnpj.Length != 14)
            {
                throw new ArgumentException($"O CNPJ '{value}' deve ter exatamente 14 dígitos.", nameof(value));
            }

            if (HasAllSameDigits(numericCnpj))
            {
                throw new ArgumentException($"O CNPJ '{value}' é inválido.", nameof(value));
            }

            if (!IsValidChecksum(numericCnpj))
            {
                throw new ArgumentException($"O CNPJ '{value}' é inválido.", nameof(value));
            }

            Value = numericCnpj;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Cnpj"/> after validation.
        /// </summary>
        /// <param name="value">The CNPJ string.</param>
        /// <returns>A new <see cref="Cnpj"/> instance.</returns>
        public static Cnpj Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new instance of <see cref="Cnpj"/>.
        /// </summary>
        /// <param name="value">The CNPJ string.</param>
        /// <param name="result">The resulting <see cref="Cnpj"/> object, or null.</param>
        /// <returns>True if created successfully; otherwise, false.</returns>
        public static bool TryCreate(string value, out Cnpj? result)
        {
            try
            {
                result = new Cnpj(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        private static bool HasAllSameDigits(string value)
        {
            var firstDigit = value[0];
            return value.All(c => c == firstDigit);
        }

        private static bool IsValidChecksum(string numericCnpj)
        {
            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCnpj = numericCnpj.Substring(0, 12);
            var sum = 0;

            for (var i = 0; i < 12; i++)
            {
                sum += (tempCnpj[i] - '0') * multiplier1[i];
            }

            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            tempCnpj += digit1;
            sum = 0;

            for (var i = 0; i < 13; i++)
            {
                sum += (tempCnpj[i] - '0') * multiplier2[i];
            }

            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return numericCnpj.EndsWith($"{digit1}{digit2}");
        }

        /// <summary>
        /// Implicitly converts a <see cref="Cnpj"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Cnpj cnpj) => cnpj?.Value ?? throw new InvalidOperationException("Não é possível converter um ValueObject nulo para string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to a <see cref="Cnpj"/> object.
        /// </summary>
        public static implicit operator Cnpj(string value) => Create(value);

        /// <summary>
        /// Returns the string representation of the CNPJ.
        /// </summary>
        public override string ToString() => Value;
    }
}
