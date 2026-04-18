using System.Runtime.CompilerServices;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding dependency injection to any class.
/// Uses ConditionalWeakTable to avoid memory leaks - objects can be garbage collected.
/// 
/// IMPORTANT: While this allows multiple service providers (one per object instance),
/// the RECOMMENDED pattern follows Microsoft's DI conventions:
/// - Configure services ONCE on your main application/host object during startup
/// - Resolve services from that single provider throughout your application
/// - Use Application.Current.GetServices() or host.GetServices() patterns
/// 
/// Advanced scenarios (multiple providers) are supported but should be used carefully:
/// - Per-window isolation in multi-window apps
/// - Plugin systems with isolated dependencies
/// - Testing scenarios with mock services
/// - Modular applications with separate service scopes
/// </summary>
public static class ServiceExtensions
{
    // ConditionalWeakTable allows objects to be garbage collected automatically
    // This prevents memory leaks while maintaining the association between instances and their context
    private static readonly ConditionalWeakTable<object, InstanceContext> _instanceContexts = new();
    
    /// <summary>
    /// Gets the service provider associated with this object instance.
    /// Returns null if no service provider has been configured.
    /// 
    /// Usage: 
    ///   var services = host.GetServices();
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <returns>The service provider, or null if not configured</returns>
    public static IServiceProvider? GetServices(this object instance)
    {
        return _instanceContexts.TryGetValue(instance, out var context) ? context.ServiceProvider : null;
    }

    /// <summary>
    /// Sets or replaces the service provider for this object instance.
    /// Pass null to remove the service provider.
    /// 
    /// Usage: 
    ///   host.SetServices(provider);
    ///   host.SetServices(null);  // Remove
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <param name="serviceProvider">The service provider to assign, or null to remove</param>
    public static void SetServices(this object instance, IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
        {
            _instanceContexts.Remove(instance);
        }
        else
        {
            if (_instanceContexts.TryGetValue(instance, out var existingContext))
            {
                var newContext = existingContext with { ServiceProvider = serviceProvider };
                _instanceContexts.AddOrUpdate(instance, newContext);
            }
            else
            {
                var newContext = new InstanceContext(serviceProvider);
                _instanceContexts.AddOrUpdate(instance, newContext);
            }
        }
    }

    /// <summary>
    /// Helper method to configure and set up the service provider for any object instance.
    /// 
    /// RECOMMENDED USAGE: Call this ONCE on your main application object during startup:
    /// - WPF: Application.Current.ConfigureServices(...) in OnStartup
    /// - MAUI: MauiApp.ConfigureServices(...) in CreateMauiApp
    /// - Console: host.ConfigureServices(...) in Program.Main
    /// - WinForms: Application.ConfigureServices(...) in Program.Main
    /// 
    /// This follows Microsoft's DI pattern of a single service provider per application.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <param name="instance">The object instance (typically your main Application/Host)</param>
    /// <param name="configureServices">Action to configure the service collection</param>
    /// <returns>The configured service provider</returns>
    public static IServiceProvider ConfigureServices<T>(this T instance, Action<IServiceCollection> configureServices) where T : class
    {
        ArgumentNullException.ThrowIfNull(configureServices);

        var services = new ServiceCollection();
        configureServices(services);
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Set the service provider in the instance context
        if (_instanceContexts.TryGetValue(instance, out var existingContext))
        {
            var newContext = existingContext with { ServiceProvider = serviceProvider };
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        else
        {
            var newContext = new InstanceContext(serviceProvider);
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        
        return serviceProvider;
    }

    /// <summary>
    /// Helper method to configure services with access to the built service provider before assignment.
    /// This allows you to perform post-build actions like validation, warming up singletons, etc.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="configureServices">Action to configure the service collection</param>
    /// <param name="postBuildAction">Action to perform after building but before assigning to instance.Services</param>
    /// <returns>The configured service provider</returns>
    public static IServiceProvider ConfigureServices<T>(
        this T instance,
        Action<IServiceCollection> configureServices,
        Action<IServiceProvider> postBuildAction) where T : class
    {
        ArgumentNullException.ThrowIfNull(configureServices);
        ArgumentNullException.ThrowIfNull(postBuildAction);

        var services = new ServiceCollection();
        configureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Perform post-build actions
        postBuildAction(serviceProvider);

        // Set the service provider in the instance context
        if (_instanceContexts.TryGetValue(instance, out var existingContext))
        {
            var newContext = existingContext with { ServiceProvider = serviceProvider };
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        else
        {
            var newContext = new InstanceContext(serviceProvider);
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        
        return serviceProvider;
    }

    /// <summary>
    /// Advanced configuration method that gives you full control over the service collection and service provider.
    /// Use this when you need to customize ServiceProviderOptions or perform complex initialization.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="configureServices">Function that receives the service collection and returns a built service provider</param>
    /// <returns>The configured service provider</returns>
    public static IServiceProvider ConfigureServicesAdvanced<T>(
        this T instance,
        Func<IServiceCollection, IServiceProvider> configureServices) where T : class
    {
        ArgumentNullException.ThrowIfNull(configureServices);

        var services = new ServiceCollection();
        var serviceProvider = configureServices(services);
        
        // Set the service provider in the instance context
        if (_instanceContexts.TryGetValue(instance, out var existingContext))
        {
            var newContext = existingContext with { ServiceProvider = serviceProvider };
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        else
        {
            var newContext = new InstanceContext(serviceProvider);
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        
        return serviceProvider;
    }

    /// <summary>
    /// Configure services and return the service collection before building.
    /// This gives you the most control - you decide when to build.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="configureServices">Action to configure the service collection</param>
    /// <returns>The configured service collection (not yet built)</returns>
    public static IServiceCollection GetServiceCollection<T>(
        this T instance,
        Action<IServiceCollection> configureServices) where T : class
    {
        ArgumentNullException.ThrowIfNull(configureServices);

        var services = new ServiceCollection();
        configureServices(services);
        return services;
    }

    /// <summary>
    /// BuildServiceProvider and assign a service collection to this instance.
    /// Use this after GetServiceCollection when you're ready to build.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="services">The service collection to build</param>
    /// <param name="options">Optional service provider options for validation, scopes, etc.</param>
    /// <returns>The built service provider</returns>
    public static IServiceProvider BuildServiceProvider<T>(
        this T instance,
        IServiceCollection services,
        ServiceProviderOptions? options = null) where T : class
    {
        var serviceProvider = options != null
            ? services.BuildServiceProvider(options)
            : services.BuildServiceProvider();

        // Set the service provider in the instance context
        if (_instanceContexts.TryGetValue(instance, out var existingContext))
        {
            var newContext = existingContext with { ServiceProvider = serviceProvider };
            _instanceContexts.AddOrUpdate(instance, newContext);
        }
        else
        {
            var newContext = new InstanceContext(serviceProvider);
            _instanceContexts.AddOrUpdate(instance, newContext);
        }

        return serviceProvider;
    }

    /// <summary>
    /// Helper method to get a required service from any object's service provider.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <returns>The requested service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is null or service is not found</exception>
    public static TService GetRequiredService<T, TService>(this T instance) where T : class where TService : notnull
    {
        // Access service provider from conditional weak table - use the same instance directly
        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use the non-generic GetService(Type) method to avoid recursive calls to our extension methods
            // This follows the same pattern as CommunityToolkit.Mvvm.Ioc
            var service = context.ServiceProvider.GetService(typeof(TService));
            if (service is null)
            {
                throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");
            }
            return (TService)service;
        }

        throw new InvalidOperationException("Service provider is not configured.");
    }

    /// <summary>
    /// Helper method to get an optional service from any object's service provider.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <returns>The requested service or null if not found</returns>
    public static TService? GetService<T, TService>(this T instance) where T : class where TService : class
    {
        // Access service provider from conditional weak table - use the same instance directly
        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use the non-generic GetService(Type) method to avoid recursive calls to our extension methods
            // This follows the same pattern as CommunityToolkit.Mvvm.Ioc
            return (TService?)context.ServiceProvider.GetService(typeof(TService));
        }
        return null;
    }

    /// <summary>
    /// Helper method to get a required keyed service from any object's service provider.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="serviceKey">The service key</param>
    /// <returns>The requested service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is null or service is not found</exception>
    public static TService GetRequiredKeyedService<T, TService>(this T instance, object? serviceKey) where T : class where TService : notnull
    {
        // Access service provider from conditional weak table - use the same instance directly
        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use Microsoft's extension method directly - this is safe since keyed services are newer and less likely to conflict
            return ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<TService>(context.ServiceProvider, serviceKey);
        }

        throw new InvalidOperationException("Service provider is not configured.");
    }

    /// <summary>
    /// Helper method to get an optional keyed service from any object's service provider.
    /// </summary>
    /// <typeparam name="T">The type of the instance</typeparam>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="serviceKey">The service key</param>
    /// <returns>The requested service or null if not found</returns>
    public static TService? GetKeyedService<T, TService>(this T instance, object? serviceKey) where T : class where TService : class
    {
        // Access service provider from conditional weak table - use the same instance directly
        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use Microsoft's extension method directly - this is safe since keyed services are newer and less likely to conflict
            return ServiceProviderKeyedServiceExtensions.GetKeyedService<TService>(context.ServiceProvider, serviceKey);
        }
        return null;
    }

    // Convenience overloads for simpler syntax when the instance type is known

    /// <summary>
    /// Convenience method to get a required keyed service with simpler syntax.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="serviceKey">The service key</param>
    /// <returns>The requested service</returns>
    public static TService GetRequiredKeyedService<TService>(this object instance, object? serviceKey) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        // Don't intercept direct IServiceProvider or IServiceScope calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<TService>(directProvider, serviceKey);
        }

        if (instance is IServiceScope scope)
        {
            return ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<TService>(scope.ServiceProvider, serviceKey);
        }

        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use Microsoft's extension method directly - this is safe since keyed services are newer and less likely to conflict
            return ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<TService>(context.ServiceProvider, serviceKey);
        }
        
        throw new InvalidOperationException("Service provider is not configured.");
    }

    /// <summary>
    /// Convenience method to get an optional keyed service with simpler syntax.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <param name="serviceKey">The service key</param>
    /// <returns>The requested service or null if not found</returns>
    public static TService? GetKeyedService<TService>(this object instance, object? serviceKey) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        // Don't intercept direct IServiceProvider or IServiceScope calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderKeyedServiceExtensions.GetKeyedService<TService>(directProvider, serviceKey);
        }

        if (instance is IServiceScope scope)
        {
            return ServiceProviderKeyedServiceExtensions.GetKeyedService<TService>(scope.ServiceProvider, serviceKey);
        }

        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use Microsoft's extension method directly - this is safe since keyed services are newer and less likely to conflict
            return ServiceProviderKeyedServiceExtensions.GetKeyedService<TService>(context.ServiceProvider, serviceKey);
        }
        return null;
    }
    
    /// <summary>
    /// Convenience method to get a required service with simpler syntax.
    /// This method only works on objects that have been configured with a service provider via ConfigureServices().
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <returns>The requested service</returns>
    public static TService GetRequiredService<TService>(this object instance) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        // Don't intercept direct IServiceProvider or IServiceScope calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderServiceExtensions.GetRequiredService<TService>(directProvider);
        }

        if (instance is IServiceScope scope)
        {
            return ServiceProviderServiceExtensions.GetRequiredService<TService>(scope.ServiceProvider);
        }

        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            // Use the non-generic GetService(Type) method to avoid recursive calls to our extension methods
            // This follows the same pattern as CommunityToolkit.Mvvm.Ioc
            var service = context.ServiceProvider.GetService(typeof(TService));
            if (service is null)
            {
                throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");
            }
            return (TService)service;
        }

        throw new InvalidOperationException(
            $"Service provider is not configured for {instance.GetType().Name}. " +
            "Call ConfigureServices() first or use serviceProvider.GetRequiredService<T>() directly.");
    }

    /// <summary>
    /// Convenience method to get an optional service with simpler syntax.
    /// This method only works on objects that have been configured with a service provider via ConfigureServices().
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve</typeparam>
    /// <param name="instance">The object instance</param>
    /// <returns>The requested service or null if not found</returns>
    public static TService? GetService<TService>(this object instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        // Don't intercept direct IServiceProvider or IServiceScope calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderServiceExtensions.GetService<TService>(directProvider);
        }

        if (instance is IServiceScope scope)
        {
            return ServiceProviderServiceExtensions.GetService<TService>(scope.ServiceProvider);
        }

        if (_instanceContexts.TryGetValue(instance, out var context) && context.ServiceProvider != null)
        {
            return context.ServiceProvider.GetService(typeof(TService)) as TService;
        }

        return null;
    }

    /// <summary>
    /// Removes the service provider for this object instance.
    /// Call this when the object is no longer needed to prevent memory leaks.
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <returns>True if the service provider was removed, false if it wasn't found</returns>
    public static bool ClearServices(this object instance)
    {
        return _instanceContexts.Remove(instance);
    }
}