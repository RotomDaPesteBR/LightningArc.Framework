namespace LightningArc.Primitives
{
    /// <summary>
    /// Represents a value object that encapsulates a primitive value.
    /// </summary>
    /// <typeparam name="T">The type of the encapsulated value.</typeparam>
    public interface IValueObject<out T>
    {
        /// <summary>
        /// Gets the primitive value encapsulated by the value object.
        /// </summary>
        T Value { get; }
    }
}
