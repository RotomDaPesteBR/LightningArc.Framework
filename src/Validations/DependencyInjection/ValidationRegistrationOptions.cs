using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection;

/// <summary>
/// Configures validator registration through dependency injection.
/// </summary>
public sealed class ValidationRegistrationOptions
{
    private readonly List<Assembly> _assemblies = [];

    /// <summary>
    /// Gets or sets the default lifetime for validator registrations.
    /// </summary>
    public ServiceLifetime DefaultLifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets the assemblies selected for validator scanning.
    /// </summary>
    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    /// <summary>
    /// Adds the assembly containing the specified type to the scan list.
    /// </summary>
    public void ScanAssemblyContaining<T>() => ScanAssembly(typeof(T).Assembly);

    /// <summary>
    /// Adds an assembly to the scan list.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    public void ScanAssembly(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
    }
}
