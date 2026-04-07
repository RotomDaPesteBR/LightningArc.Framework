namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Password strength levels used with <see cref="Password"/>.
/// </summary>
public enum PasswordStrength
{
    /// <summary>
    /// Minimum strength — 8+ chars, 1 digit, 1 uppercase, 1 lowercase.
    /// </summary>
    Weak = 0,

    /// <summary>
    /// Moderate strength — meets Weak requirements plus 10+ chars and 1 special character.
    /// </summary>
    Moderate = 1,

    /// <summary>
    /// Strong password — 14+ chars, mixed case, digits, special characters.
    /// </summary>
    Strong = 2,
}
