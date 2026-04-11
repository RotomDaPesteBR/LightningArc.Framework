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

        internal Cnpj(string value)
        {
            if (!IsValid(value, out string? errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            Value = new string([.. value.Where(char.IsDigit)]);
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
            if (IsValid(value, out _))
            {
                result = new Cnpj(value);
                return true;
            }

            result = null;
            return false;
        }

        internal static bool IsValid(string value, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = "The CNPJ cannot be null or empty.";
                return false;
            }

            string numericCnpj = new([.. value.Where(char.IsDigit)]);

            if (numericCnpj.Length != 14)
            {
                errorMessage = $"The CNPJ '{value}' must have exactly 14 digits.";
                return false;
            }

            if (HasAllSameDigits(numericCnpj) || !IsValidChecksum(numericCnpj))
            {
                errorMessage = $"The CNPJ '{value}' is invalid.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool HasAllSameDigits(string value)
        {
            char firstDigit = value[0];
            return value.All(c => c == firstDigit);
        }

        private static bool IsValidChecksum(string numericCnpj)
        {
            int[] multiplier1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplier2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            string tempCnpj = numericCnpj[..12];
            int sum = 0;

            for (int i = 0; i < 12; i++)
            {
                sum += (tempCnpj[i] - '0') * multiplier1[i];
            }

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            tempCnpj += digit1;
            sum = 0;

            for (int i = 0; i < 13; i++)
            {
                sum += (tempCnpj[i] - '0') * multiplier2[i];
            }

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return numericCnpj.EndsWith($"{digit1}{digit2}");
        }

        /// <summary>
        /// Implicitly converts a <see cref="Cnpj"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Cnpj cnpj) => cnpj?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

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
