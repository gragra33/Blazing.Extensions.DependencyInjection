using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for automatic service discovery and registration.
/// </summary>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Automatically discovers and registers all implementations of the specified interface type
    /// from the assemblies added via AddAssembly/AddAssemblies with the specified service lifetime.
    /// If no assemblies have been added, uses the calling assembly.
    /// 
    /// Usage examples:
    /// - host.AddAssembly(assembly).ConfigureServices(services => services.Register&lt;ITabView&gt;(ServiceLifetime.Transient))
    /// - instance.Register&lt;IRepository&gt;(ServiceLifetime.Scoped)
    /// </summary>
    /// <typeparam name="T">The type of the object instance</typeparam>
    /// <typeparam name="TInterface">The interface type to discover implementations for</typeparam>
    /// <param name="instance">The object instance that has a service collection</param>
    /// <param name="lifetime">The service lifetime (Transient, Singleton, or Scoped)</param>
    /// <returns>The object instance for fluent chaining</returns>
    public static T Register<T, TInterface>(this T instance, ServiceLifetime lifetime) 
        where T : class 
        where TInterface : class
    {
        // Get the service collection from the instance
        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            throw new InvalidOperationException(
                "Service provider is not configured. Call ConfigureServices() or GetServiceCollection() first.");
        }

        // If this is called during service configuration, we need to work with IServiceCollection
        // For now, let's throw a more helpful error
        throw new InvalidOperationException(
            "Register<TInterface> should be called on IServiceCollection during service configuration. " +
            "Use services.Register<TInterface>(lifetime) instead of instance.Register<TInterface>().");
    }

    /// <summary>
    /// Automatically discovers and registers all implementations of the specified interface type
    /// from the assemblies added via AddAssembly/AddAssemblies with the specified service lifetime.
    /// If no assemblies have been added, uses the calling assembly.
    /// 
    /// Usage examples:
    /// - services.Register&lt;ITabView&gt;(ServiceLifetime.Transient)
    /// - host.AddAssembly(assembly1).AddAssembly(assembly2); host.ConfigureServices(services => services.Register&lt;IRepository&gt;(ServiceLifetime.Scoped))
    /// </summary>
    /// <typeparam name="TInterface">The interface type to discover implementations for</typeparam>
    /// <param name="services">The service collection to register services in</param>
    /// <param name="lifetime">The service lifetime (Transient, Singleton, or Scoped)</param>
    /// <returns>The service collection for fluent chaining</returns>
    public static IServiceCollection Register<TInterface>(this IServiceCollection services, ServiceLifetime lifetime)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // Get assemblies from the current instance context or use calling assembly as fallback
        var assemblies = ServiceExtensions.GetCurrentInstanceAssemblies();
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        // Discover all implementations of TInterface in the specified assemblies
        var interfaceType = typeof(TInterface);
        var implementations = new List<Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(type => 
                        type is { IsClass: true, IsAbstract: false, IsInterface: false } &&
                        interfaceType.IsAssignableFrom(type))
                    .ToList();

                implementations.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle assemblies that might have loading issues
                var loadedTypes = ex.Types.Where(t => t != null).Cast<Type>();
                var validTypes = loadedTypes
                    .Where(type => 
                        type.IsClass && 
                        type is { IsAbstract: false, IsInterface: false } &&
                        interfaceType.IsAssignableFrom(type))
                    .ToList();

                implementations.AddRange(validTypes);
            }
        }

        // Register each implementation with the specified scope
        foreach (var implementationType in implementations)
        {
            // Register the concrete type
            RegisterWithLifetime(services, implementationType, implementationType, lifetime);
            
            // Register as the interface type
            RegisterWithLifetime(services, interfaceType, implementationType, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all classes decorated with the AutoRegisterAttribute
    /// from the assemblies added via AddAssembly/AddAssemblies to the current instance context.
    /// If no assemblies have been added, uses the calling assembly.
    /// 
    /// Usage examples:
    /// - services.Register() // Scans assemblies from current instance context
    /// - host.AddAssembly(assembly1).AddAssembly(assembly2); host.ConfigureServices(services => services.Register())
    /// </summary>
    /// <param name="services">The service collection to register services in</param>
    /// <returns>The service collection for fluent chaining</returns>
    public static IServiceCollection Register(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Get assemblies from the current instance context or use calling assembly as fallback
        var assemblies = ServiceExtensions.GetCurrentInstanceAssemblies();
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }
        return services.Register(assemblies);
    }

    /// <summary>
    /// Automatically discovers and registers all classes decorated with the AutoRegisterAttribute
    /// from the specified assembly. Classes will be registered according to their attribute configuration.
    /// 
    /// Usage examples:
    /// - services.Register(typeof(MyClass).Assembly)
    /// </summary>
    /// <param name="services">The service collection to register services in</param>
    /// <param name="assembly">The assembly to scan for AutoRegisterAttribute decorated classes</param>
    /// <returns>The service collection for fluent chaining</returns>
    public static IServiceCollection Register(this IServiceCollection services, Assembly assembly)
    {
        return services.Register([assembly]);
    }

    /// <summary>
    /// Automatically discovers and registers all classes decorated with the AutoRegisterAttribute
    /// from the specified assemblies. Classes will be registered according to their attribute configuration.
    /// 
    /// Usage examples:
    /// - services.Register(assembly1, assembly2, assembly3)
    /// - services.Register([assembly1, assembly2])
    /// </summary>
    /// <param name="services">The service collection to register services in</param>
    /// <param name="assemblies">The assemblies to scan for AutoRegisterAttribute decorated classes</param>
    /// <returns>The service collection for fluent chaining</returns>
    public static IServiceCollection Register(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            try
            {
                var typesWithAttribute = assembly.GetTypes()
                    .Where(type => 
                        type is { IsClass: true, IsAbstract: false, IsInterface: false } &&
                        type.GetCustomAttribute<AutoRegisterAttribute>() != null)
                    .ToList();

                foreach (var implementationType in typesWithAttribute)
                {
                    var attribute = implementationType.GetCustomAttribute<AutoRegisterAttribute>()!;
                    
                    if (attribute.ServiceType != null)
                    {
                        // Register as the specified service type
                        RegisterWithLifetime(services, attribute.ServiceType, implementationType, attribute.Lifetime);
                        
                        // Also register as the concrete type if different from service type
                        if (attribute.ServiceType != implementationType)
                        {
                            RegisterWithLifetime(services, implementationType, implementationType, attribute.Lifetime);
                        }
                    }
                    else
                    {
                        // Register as self
                        RegisterWithLifetime(services, implementationType, implementationType, attribute.Lifetime);
                        
                        // Register as all implemented interfaces
                        var interfaces = implementationType.GetInterfaces()
                            .Where(i => i != typeof(IDisposable) && !i.IsGenericTypeDefinition);
                        
                        foreach (var interfaceType in interfaces)
                        {
                            RegisterWithLifetime(services, interfaceType, implementationType, attribute.Lifetime);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle assemblies that might have loading issues
                var loadedTypes = ex.Types.Where(t => t != null).Cast<Type>();
                var typesWithAttribute = loadedTypes
                    .Where(type => 
                        type.IsClass && 
                        type is { IsAbstract: false, IsInterface: false } &&
                        type.GetCustomAttribute<AutoRegisterAttribute>() != null)
                    .ToList();

                foreach (var implementationType in typesWithAttribute)
                {
                    var attribute = implementationType.GetCustomAttribute<AutoRegisterAttribute>()!;
                    
                    if (attribute.ServiceType != null)
                    {
                        // Register as the specified service type
                        RegisterWithLifetime(services, attribute.ServiceType, implementationType, attribute.Lifetime);
                        
                        // Also register as the concrete type if different from service type
                        if (attribute.ServiceType != implementationType)
                        {
                            RegisterWithLifetime(services, implementationType, implementationType, attribute.Lifetime);
                        }
                    }
                    else
                    {
                        // Register as self
                        RegisterWithLifetime(services, implementationType, implementationType, attribute.Lifetime);
                        
                        // Register as all implemented interfaces
                        var interfaces = implementationType.GetInterfaces()
                            .Where(i => i != typeof(IDisposable) && !i.IsGenericTypeDefinition);
                        
                        foreach (var interfaceType in interfaces)
                        {
                            RegisterWithLifetime(services, interfaceType, implementationType, attribute.Lifetime);
                        }
                    }
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Helper method to register a service with the specified lifetime.
    /// </summary>
    private static void RegisterWithLifetime(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime");
        }
    }
}
