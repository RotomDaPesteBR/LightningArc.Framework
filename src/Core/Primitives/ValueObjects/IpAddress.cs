using System.Net;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects;

/// <summary>
/// Represents a validated IPv4 or IPv6 address.
/// </summary>
public record IpAddress : IValueObject<string>
{
    /// <summary>
    /// Gets the string value of the IP address.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the IP version (4 or 6).
    /// </summary>
    public int Version { get; }

    private IPAddress? Address { get; }

    private IpAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The IP address cannot be null or empty.", nameof(value));
        }

        if (!IPAddress.TryParse(value, out var address))
        {
            throw new ArgumentException($"The value '{value}' is not a valid IP address.", nameof(value));
        }

        Value = address.ToString();
        Address = address;
        Version = address.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => 4,
            System.Net.Sockets.AddressFamily.InterNetworkV6 => 6,
            _ => 0
        };
    }

    /// <summary>
    /// Creates a new <see cref="IpAddress"/> from the specified string value.
    /// </summary>
    public static IpAddress Create(string value) => new(value);

    /// <summary>
    /// Tries to create a new <see cref="IpAddress"/> from the specified string value.
    /// </summary>
    public static bool TryCreate(string value, out IpAddress? result)
    {
        try
        {
            result = new IpAddress(value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts an <see cref="IpAddress"/> to its <see cref="string"/> representation.
    /// </summary>
    public static implicit operator string(IpAddress ipAddress) => ipAddress?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

    /// <inheritdoc/>
    public override string ToString() => Value;
}
