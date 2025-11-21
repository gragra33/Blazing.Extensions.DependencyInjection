using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for service collection validation and diagnostics.
/// Provides methods to analyze, validate, and report on service registrations.
/// 
/// Service validation is useful for:
/// - Detecting duplicate service registrations
/// - Finding circular dependencies
/// - Identifying unused services
/// - Validating lifetime compatibility
/// - Analyzing service dependency graphs
/// 
/// Usage:
///   var duplicates = services.ValidateDuplicateRegistrations();
///   var violations = services.ValidateLifetimeCompatibility();
///   var graph = services.GetServiceDependencyGraph();
/// </summary>
public static class ServiceValidationExtensions
{
    /// <summary>
    /// Validates a service collection for duplicate registrations.
    /// Returns information about duplicate service types.
    /// 
    /// Usage:
    ///   var duplicates = services.ValidateDuplicateRegistrations();
    ///   foreach (var dup in duplicates)
    ///   {
    ///       Console.WriteLine($"Service {dup.ServiceType.Name} has {dup.Registrations.Count} registrations");
    ///   }
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <returns>Enumerable of duplicate registration info</returns>
    public static IEnumerable<ServiceDuplicateInfo> ValidateDuplicateRegistrations(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var grouped = services
            .GroupBy(sd => sd.ServiceType)
            .Where(g => g.Count() > 1);

        var result = new List<ServiceDuplicateInfo>();

        foreach (var group in grouped)
        {
            var dup = new ServiceDuplicateInfo { Count = group.Count(), IsKeyed = false };
            foreach (var reg in group)
            {
                ((Collection<ServiceDescriptor>)dup.Registrations).Add(reg);
            }
            result.Add(dup);
        }

        return result;
    }

    /// <summary>
    /// Detects circular dependencies in the service collection.
    /// Uses graph traversal to identify cycles.
    /// 
    /// Usage:
    ///   var cycles = services.ValidateCircularDependencies();
    ///   if (cycles.Count > 0)
    ///   {
    ///       foreach (var cycle in cycles)
    ///           Console.WriteLine($"Cycle detected: {cycle.Description}");
    ///   }
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <returns>Enumerable of circular dependency info</returns>
    public static IEnumerable<ServiceCircularDependencyInfo> ValidateCircularDependencies(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var dependencyGraph = BuildDependencyGraph(services);
        var cycles = new List<ServiceCircularDependencyInfo>();

        foreach (var serviceType in dependencyGraph.Keys)
        {
            var cycle = FindCycle(serviceType, dependencyGraph, new HashSet<Type>(), new List<Type>());
            if (cycle != null)
            {
                var cycleDep = new ServiceCircularDependencyInfo
                {
                    Description = $"Circular dependency: {string.Join(" ? ", cycle.Select(t => t.Name))} ? {cycle[0].Name}"
                };
                foreach (var type in cycle)
                {
                    ((Collection<Type>)cycleDep.CyclePath).Add(type);
                }
                cycles.Add(cycleDep);
            }
        }

        return cycles;
    }

    /// <summary>
    /// Validates lifetime compatibility across the service collection.
    /// Detects scoped-in-singleton and other violations.
    /// 
    /// Usage:
    ///   var violations = services.ValidateLifetimeCompatibility();
    ///   foreach (var violation in violations)
    ///       Console.WriteLine($"Lifetime violation: {violation.ErrorMessage}");
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <returns>Enumerable of lifetime validation errors</returns>
    public static IEnumerable<ServiceLifetimeValidationError> ValidateLifetimeCompatibility(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var errors = new List<ServiceLifetimeValidationError>();
        var descriptorMap = services.ToDictionary(sd => sd.ServiceType, sd => sd);

        foreach (var descriptor in services)
        {
            // Skip abstract types and interfaces without implementation
            if (descriptor.ServiceType.IsAbstract || descriptor.ServiceType.IsInterface)
            {
                continue;
            }

            var implementationType = descriptor.ImplementationType ?? descriptor.ServiceType;

            // Get constructor parameters
            var constructors = implementationType.GetConstructors();
            if (constructors.Length == 0)
            {
                continue;
            }

            // Use the constructor with the most parameters
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (!descriptorMap.TryGetValue(paramType, out var dependencyDescriptor))
                {
                    continue;
                }

                // Validate lifetime compatibility
                if (descriptor.Lifetime == ServiceLifetime.Singleton && 
                    dependencyDescriptor.Lifetime == ServiceLifetime.Scoped)
                {
                    errors.Add(new ServiceLifetimeValidationError
                    {
                        ServiceType = descriptor.ServiceType,
                        ActualLifetime = descriptor.Lifetime,
                        RequiredLifetime = ServiceLifetime.Scoped,
                        DependentType = paramType,
                        DependentLifetime = dependencyDescriptor.Lifetime,
                        ErrorMessage = 
                            $"Service {descriptor.ServiceType.Name} registered as Singleton depends on " +
                            $"{paramType.Name} which is registered as Scoped. " +
                            $"Singletons cannot depend on scoped services. " +
                            $"Consider making {descriptor.ServiceType.Name} Scoped or {paramType.Name} Transient."
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets the complete service dependency graph.
    /// Useful for visualization and analysis.
    /// 
    /// Usage:
    ///   var graph = services.GetServiceDependencyGraph();
    ///   Console.WriteLine($"Total services: {graph.Services.Count}");
    ///   Console.WriteLine($"Total dependencies: {graph.Dependencies.Count}");
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>Complete dependency graph</returns>
    public static ServiceDependencyGraph GetServiceDependencyGraph(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var dependencyMap = BuildDependencyGraph(services);
        var registrationMap = services.GroupBy(sd => sd.ServiceType).ToDictionary(
            g => g.Key,
            g => g.ToList());

        var serviceList = services.Select(sd => new ServiceInfo
        {
            ServiceType = sd.ServiceType,
            ImplementationType = sd.ImplementationType ?? sd.ServiceType,
            Lifetime = sd.Lifetime,
            Registrations = registrationMap.TryGetValue(sd.ServiceType, out var regs)
                ? regs.Count 
                : 1
        }).Distinct(new ServiceInfoComparer()).ToList();

        var dependencyDict = new Dictionary<Type, ICollection<ServiceDependency>>();
        foreach (var kvp in dependencyMap)
        {
            var collection = new Collection<ServiceDependency>();
            foreach (var t in kvp.Value)
            {
                collection.Add(new ServiceDependency
                {
                    DependencyType = t,
                    IsRequired = true
                });
            }
            dependencyDict[kvp.Key] = collection;
        }

        var graph = new ServiceDependencyGraph();
        foreach (var service in serviceList)
        {
            ((Collection<ServiceInfo>)graph.Services).Add(service);
        }
        
        var dict = (Dictionary<Type, ICollection<ServiceDependency>>)graph.Dependencies;
        foreach (var kvp in dependencyDict)
        {
            dict.Add(kvp.Key, kvp.Value);
        }

        return graph;
    }

    /// <summary>
    /// Gets diagnostic information about a service collection.
    /// Provides summary statistics and warnings.
    /// 
    /// Usage:
    ///   var diagnostics = services.GetDiagnostics();
    ///   Console.WriteLine($"Total services: {diagnostics.TotalServices}");
    ///   Console.WriteLine($"Warnings: {diagnostics.Warnings.Count}");
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>Diagnostic information</returns>
    public static ServiceCollectionDiagnostics GetDiagnostics(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var duplicates = ValidateDuplicateRegistrations(services).ToList();
        var circular = ValidateCircularDependencies(services).ToList();
        var violations = ValidateLifetimeCompatibility(services).ToList();

        var diagnostics = new ServiceCollectionDiagnostics
        {
            TotalServices = services.Count,
            SingletonCount = services.Count(sd => sd.Lifetime == ServiceLifetime.Singleton),
            ScopedCount = services.Count(sd => sd.Lifetime == ServiceLifetime.Scoped),
            TransientCount = services.Count(sd => sd.Lifetime == ServiceLifetime.Transient),
            KeyedServices = 0
        };

        // Add duplicates
        foreach (var dup in duplicates)
        {
            ((Collection<ServiceDuplicateInfo>)diagnostics.Duplicates).Add(dup);
        }

        // Add circular dependencies
        foreach (var circ in circular)
        {
            ((Collection<ServiceCircularDependencyInfo>)diagnostics.CircularDependencies).Add(circ);
        }

        // Add violations
        foreach (var viol in violations)
        {
            ((Collection<ServiceLifetimeValidationError>)diagnostics.LifetimeViolations).Add(viol);
        }

        // Add warnings
        if (diagnostics.Duplicates.Count > 0)
        {
            ((Collection<string>)diagnostics.Warnings).Add($"Found {diagnostics.Duplicates.Count} duplicate service registrations");
        }

        if (diagnostics.CircularDependencies.Count > 0)
        {
            ((Collection<string>)diagnostics.Warnings).Add($"Found {diagnostics.CircularDependencies.Count} circular dependencies");
        }

        if (diagnostics.LifetimeViolations.Count > 0)
        {
            ((Collection<string>)diagnostics.Warnings).Add($"Found {diagnostics.LifetimeViolations.Count} lifetime compatibility violations");
        }

        if (diagnostics.SingletonCount > 20)
        {
            ((Collection<string>)diagnostics.Warnings).Add($"High number of singletons ({diagnostics.SingletonCount}). Consider using scoped services.");
        }

        return diagnostics;
    }

    /// <summary>
    /// Validates the entire service collection and throws if validation fails.
    /// Useful for startup validation.
    /// 
    /// Usage:
    ///   try
    ///   {
    ///       services.ThrowIfInvalid();
    ///       // Validation passed
    ///   }
    ///   catch (InvalidOperationException ex)
    ///   {
    ///       // Handle validation failure
    ///   }
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <exception cref="InvalidOperationException">Thrown if validation fails</exception>
    public static void ThrowIfInvalid(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var violations = ValidateLifetimeCompatibility(services).ToList();
        if (violations.Count > 0)
        {
            var message = string.Join(
                Environment.NewLine,
                violations.Select(v => $"  • {v.ErrorMessage}"));
            throw new InvalidOperationException(
                $"Service collection validation failed:{Environment.NewLine}{message}");
        }

        var cycles = ValidateCircularDependencies(services).ToList();
        if (cycles.Count > 0)
        {
            var message = string.Join(
                Environment.NewLine,
                cycles.Select(c => $"  • {c.Description}"));
            throw new InvalidOperationException(
                $"Circular dependencies detected:{Environment.NewLine}{message}");
        }
    }

    // Private helper methods

    private static Dictionary<Type, List<Type>> BuildDependencyGraph(IServiceCollection services)
    {
        var graph = new Dictionary<Type, List<Type>>();

        foreach (var descriptor in services)
        {
            var implementationType = descriptor.ImplementationType ?? descriptor.ServiceType;

            if (!graph.ContainsKey(implementationType))
            {
                graph[implementationType] = new List<Type>();
            }

            var constructors = implementationType.GetConstructors();
            if (constructors.Length == 0)
            {
                continue;
            }

            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (services.Any(sd => sd.ServiceType == paramType))
                {
                    if (!graph[implementationType].Contains(paramType))
                    {
                        graph[implementationType].Add(paramType);
                    }
                }
            }
        }

        return graph;
    }

    private static List<Type>? FindCycle(
        Type current,
        Dictionary<Type, List<Type>> graph,
        HashSet<Type> visited,
        List<Type> path)
    {
        if (visited.Contains(current))
        {
            if (path.Contains(current))
            {
                var cycleStart = path.IndexOf(current);
                return path.Skip(cycleStart).Append(current).ToList();
            }
            return null;
        }

        visited.Add(current);
        path.Add(current);

        if (graph.TryGetValue(current, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                var cycle = FindCycle(dependency, graph, new HashSet<Type>(visited), new List<Type>(path));
                if (cycle != null)
                {
                    return cycle;
                }
            }
        }

        path.Remove(current);
        return null;
    }
}

// Supporting classes

/// <summary>
/// Information about duplicate service registrations.
/// </summary>
public class ServiceDuplicateInfo
{
    /// <summary>
    /// All registrations for this service type.
    /// </summary>
    public ICollection<ServiceDescriptor> Registrations { get; } = new Collection<ServiceDescriptor>();

    /// <summary>
    /// Number of registrations.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Whether this is a keyed service registration.
    /// </summary>
    public bool IsKeyed { get; set; }
}

/// <summary>
/// Information about circular dependencies.
/// </summary>
public class ServiceCircularDependencyInfo
{
    /// <summary>
    /// The path of the cycle.
    /// </summary>
    public ICollection<Type> CyclePath { get; } = new Collection<Type>();

    /// <summary>
    /// Human-readable description of the cycle.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Information about a lifetime compatibility violation.
/// </summary>
public class ServiceLifetimeValidationError
{
    /// <summary>
    /// The service type with the violation.
    /// </summary>
    public Type ServiceType { get; set; } = null!;

    /// <summary>
    /// The actual lifetime of the service.
    /// </summary>
    public ServiceLifetime ActualLifetime { get; set; }

    /// <summary>
    /// The required lifetime based on dependencies.
    /// </summary>
    public ServiceLifetime RequiredLifetime { get; set; }

    /// <summary>
    /// The dependent type that causes the violation.
    /// </summary>
    public Type DependentType { get; set; } = null!;

    /// <summary>
    /// The lifetime of the dependent type.
    /// </summary>
    public ServiceLifetime DependentLifetime { get; set; }

    /// <summary>
    /// Human-readable error message with resolution suggestions.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Complete service dependency graph.
/// </summary>
public class ServiceDependencyGraph
{
    /// <summary>
    /// All services in the collection.
    /// </summary>
    public ICollection<ServiceInfo> Services { get; } = new Collection<ServiceInfo>();

    /// <summary>
    /// Dependency relationships between services.
    /// Key: Service type, Value: List of its dependencies.
    /// </summary>
    public IDictionary<Type, ICollection<ServiceDependency>> Dependencies { get; } = 
        new Dictionary<Type, ICollection<ServiceDependency>>();
}

/// <summary>
/// Information about a single service.
/// </summary>
public class ServiceInfo
{
    /// <summary>
    /// The service interface type.
    /// </summary>
    public Type ServiceType { get; set; } = null!;

    /// <summary>
    /// The implementation type.
    /// </summary>
    public Type ImplementationType { get; set; } = null!;

    /// <summary>
    /// The service lifetime.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Number of registrations for this service.
    /// </summary>
    public int Registrations { get; set; } = 1;
}

/// <summary>
/// Information about a service dependency.
/// </summary>
public class ServiceDependency
{
    /// <summary>
    /// The dependency type.
    /// </summary>
    public Type DependencyType { get; set; } = null!;

    /// <summary>
    /// Whether the dependency is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Diagnostic information about a service collection.
/// </summary>
public class ServiceCollectionDiagnostics
{
    /// <summary>
    /// Total number of services registered.
    /// </summary>
    public int TotalServices { get; set; }

    /// <summary>
    /// Number of singleton services.
    /// </summary>
    public int SingletonCount { get; set; }

    /// <summary>
    /// Number of scoped services.
    /// </summary>
    public int ScopedCount { get; set; }

    /// <summary>
    /// Number of transient services.
    /// </summary>
    public int TransientCount { get; set; }

    /// <summary>
    /// Number of keyed services.
    /// </summary>
    public int KeyedServices { get; set; }

    /// <summary>
    /// Duplicate registrations found.
    /// </summary>
    public ICollection<ServiceDuplicateInfo> Duplicates { get; } = new Collection<ServiceDuplicateInfo>();

    /// <summary>
    /// Circular dependencies found.
    /// </summary>
    public ICollection<ServiceCircularDependencyInfo> CircularDependencies { get; } = 
        new Collection<ServiceCircularDependencyInfo>();

    /// <summary>
    /// Lifetime compatibility violations found.
    /// </summary>
    public ICollection<ServiceLifetimeValidationError> LifetimeViolations { get; } = 
        new Collection<ServiceLifetimeValidationError>();

    /// <summary>
    /// Warning messages generated during validation.
    /// </summary>
    public ICollection<string> Warnings { get; } = new Collection<string>();

    /// <summary>
    /// Whether the collection is valid.
    /// </summary>
    public bool IsValid => Duplicates.Count == 0 && 
                          CircularDependencies.Count == 0 && 
                          LifetimeViolations.Count == 0;
}

// Helper comparer for ServiceInfo
internal sealed class ServiceInfoComparer : IEqualityComparer<ServiceInfo>
{
    public bool Equals(ServiceInfo? x, ServiceInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.ServiceType == y.ServiceType && x.ImplementationType == y.ImplementationType;
    }

    public int GetHashCode(ServiceInfo obj)
    {
        return HashCode.Combine(obj.ServiceType, obj.ImplementationType);
    }
}
