using System.Globalization;
using System.Resources;

namespace LightningArc.Results.Localization;

/// <summary>
/// Manages localization for error messages within the library.
/// This class can be optionally configured by the consumer to set a specific culture
/// and resource manager for error message lookup.
/// </summary>
public static class LocalizationManager
{
    private static ResourceManager _errorResourceManager = new(
        "LightningArc.Results.Resources.ErrorMessages",
        typeof(LocalizationManager).Assembly
    );
    private static ResourceManager _successResourceManager = new(
        "LightningArc.Results.Resources.SuccessMessages",
        typeof(LocalizationManager).Assembly
    );

    /// <summary>
    /// Gets the currently configured culture for messages.
    /// Defaults to <see cref="CultureInfo.InvariantCulture"/> if not explicitly configured.
    /// </summary>
    public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Configures the localization settings for messages.
    /// This method should be called once at application startup if custom localization is desired.
    /// </summary>
    /// <param name="cultureName">The name of the culture to use (e.g., "en-US", "pt-BR").</param>
    /// <param name="errorResourceManager">Optional. A custom <see cref="ResourceManager"/> to use for error message lookup.
    /// If not provided, the library's default error resource manager will be used.</param>
    /// <param name="successResourceManager">Optional. A custom <see cref="ResourceManager"/> to use for success message lookup.
    /// If not provided, the library's default success resource manager will be used.</param>
    public static void Configure(
        string cultureName,
        ResourceManager? errorResourceManager = null,
        ResourceManager? successResourceManager = null
    )
    {
        CurrentCulture = new CultureInfo(cultureName);
        if (errorResourceManager != null)
        {
            _errorResourceManager = errorResourceManager;
        }
        if (successResourceManager != null)
        {
            _successResourceManager = successResourceManager;
        }
    }

    /// <summary>
    /// Retrieves a localized string for the given error resource key.
    /// </summary>
    /// <param name="key">The resource key for the error message.</param>
    /// <returns>The localized string, or the key itself if the resource is not found.</returns>
    internal static string GetErrorString(string key)
    {
        return _errorResourceManager.GetString(key, CurrentCulture) ?? key;
    }

    /// <summary>
    /// Retrieves a localized string for the given success resource key.
    /// </summary>
    /// <param name="key">The resource key for the success message.</param>
    /// <returns>The localized string, or the key itself if the resource is not found.</returns>
    internal static string GetSuccessString(string key)
    {
        return _successResourceManager.GetString(key, CurrentCulture) ?? key;
    }
}
