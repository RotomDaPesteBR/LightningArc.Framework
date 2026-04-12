using LightningArc.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net;

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

    internal IpAddress(string value)
    {
        if (!IsValid(value, out string? errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(value));
        }

        if (!IPAddress.TryParse(value, out IPAddress? address))
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
    public static bool TryCreate(string value, [NotNullWhen(true)] out IpAddress? result)
    {
        if (IsValid(value, out _))
        {
            result = new IpAddress(value);
            return true;
        }

        result = null;
        return false;
    }

    internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "The IP address cannot be null or empty.";
            return false;
        }

        if (!IPAddress.TryParse(value, out _))
        {
            errorMessage = $"The value '{value}' is not a valid IP address.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Implicitly converts an <see cref="IpAddress"/> to its <see cref="string"/> representation.
    /// </summary>
    public static implicit operator string(IpAddress ipAddress) => ipAddress?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

    /// <inheritdoc/>
    public override string ToString() => Value;
}
