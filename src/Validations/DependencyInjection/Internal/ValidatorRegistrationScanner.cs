using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection.Internal;

internal static class ValidatorRegistrationScanner
{
    public static void RegisterFromAssemblies(
        IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime)
    {
        foreach (Assembly assembly in assemblies.Distinct())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
                {
                    continue;
                }

                Type[] contracts = type
                    .GetInterfaces()
                    .Where(interfaceType =>
                        interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray();

                foreach (Type contract in contracts)
                {
                    services.Add(new ServiceDescriptor(contract, type, lifetime));
                }
            }
        }
    }
}
