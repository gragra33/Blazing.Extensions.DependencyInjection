using Blazing.Extensions.DependencyInjection;

namespace MultiTenantExample.Server.Extensions;

/// <summary>
/// Extension methods for service validation and diagnostics.
/// </summary>
public static class ServiceValidationExtensions
{
    /// <summary>
    /// Validates the service collection and prints diagnostics to the console.
    /// </summary>
    public static IServiceCollection ValidateAndPrintDiagnostics(this IServiceCollection services)
    {
        // 5. Service Validation - Detect configuration issues at startup
        var diagnostics = services.GetDiagnostics();
        Console.WriteLine($"+ Service Collection Diagnostics:");
        Console.WriteLine($"  Total Services: {diagnostics.TotalServices}");
        Console.WriteLine($"  Singletons: {diagnostics.SingletonCount}");
        Console.WriteLine($"  Scoped: {diagnostics.ScopedCount}");
        Console.WriteLine($"  Transient: {diagnostics.TransientCount}");

        if (diagnostics.Warnings.Count > 0)
        {
            PrintWarnings(diagnostics.Warnings);
            PrintDuplicateRegistrations(services);
            PrintSingletonRegistrations(services);
        }

        // Validate for circular dependencies and lifetime violations
        try
        {
            services.ThrowIfInvalid();
            Console.WriteLine("\n+ Service collection validation passed");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"\nX Service collection validation failed: {ex.Message}");
            throw;
        }

        return services;
    }

    private static void PrintWarnings(ICollection<string> warnings)
    {
        Console.WriteLine($"  ! Warnings: {warnings.Count}");
        foreach (var warning in warnings)
        {
            Console.WriteLine($"    - {warning}");
        }
    }

    private static void PrintDuplicateRegistrations(IServiceCollection services)
    {
        var duplicates = services.ValidateDuplicateRegistrations();
        if (!duplicates.Any()) return;

        Console.WriteLine($"\n  Duplicate Service Registrations ({duplicates.Count()} types):");
        foreach (var dup in duplicates.Take(10)) // Limit to first 10
        {
            var serviceType = dup.Registrations.First().ServiceType;
            var serviceTypeName = GetFullTypeName(serviceType);
            Console.WriteLine($"    > {serviceTypeName} ({dup.Count} registrations):");
            
            foreach (var registration in dup.Registrations.Take(3)) // Show first 3 implementations
            {
                var implName = GetImplementationName(registration);
                Console.WriteLine($"      - {implName} ({registration.Lifetime})");
            }
            
            if (dup.Count > 3)
            {
                Console.WriteLine($"      ... and {dup.Count - 3} more");
            }
        }
        
        if (duplicates.Count() > 10)
        {
            Console.WriteLine($"    ... and {duplicates.Count() - 10} more duplicate types");
        }
    }

    private static void PrintSingletonRegistrations(IServiceCollection services)
    {
        var singletons = services
            .Where(sd => sd.Lifetime == ServiceLifetime.Singleton)
            .OrderBy(sd => GetFullTypeName(sd.ServiceType))
            .ToList();

        if (!singletons.Any()) return;

        Console.WriteLine($"\n  All Singleton Service Registrations ({singletons.Count} total):");
        foreach (var singleton in singletons)
        {
            var serviceTypeName = GetFullTypeName(singleton.ServiceType);
            var implTypeName = GetImplementationName(singleton);
            var keyInfo = singleton.ServiceKey != null ? $" [Key: {singleton.ServiceKey}]" : "";
            Console.WriteLine($"    - {serviceTypeName} -> {implTypeName}{keyInfo}");
        }
    }

    private static string GetImplementationName(ServiceDescriptor descriptor)
    {
        return descriptor.ImplementationType != null
            ? GetFullTypeName(descriptor.ImplementationType)
            : descriptor.ImplementationFactory != null
                ? "Factory"
                : descriptor.ImplementationInstance != null
                    ? $"Instance of {GetFullTypeName(descriptor.ImplementationInstance.GetType())}"
                    : "Unknown";
    }

    private static string GetFullTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypeName = type.GetGenericTypeDefinition().FullName ?? type.Name;
            // Remove the backtick and number (e.g., `1)
            var backtickIndex = genericTypeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                genericTypeName = genericTypeName.Substring(0, backtickIndex);
            }

            var genericArgs = type.GetGenericArguments();
            var genericArgNames = string.Join(", ", genericArgs.Select(GetFullTypeName));
            return $"{genericTypeName}<{genericArgNames}>";
        }

        return type.FullName ?? type.Name;
    }
}
