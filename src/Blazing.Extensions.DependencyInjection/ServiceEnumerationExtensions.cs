namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for service enumeration with Blazing.Extensions.DependencyInjection.
/// Provides convenient methods for resolving multiple implementations of a service type.
/// 
/// Service enumeration is useful for:
/// - Plugin-based architectures
/// - Middleware chains
/// - Command/handler patterns with multiple implementations
/// - Composite patterns
/// 
/// Usage:
///   var handlers = instance.GetRequiredServices&lt;ICommandHandler&gt;();
///   foreach (var handler in handlers)
///   {
///       handler.Handle(command);
///   }
/// </summary>
public static class ServiceEnumerationExtensions
{
    /// <summary>
    /// Gets all registered implementations of a service type.
    /// 
    /// Usage:
    ///   var handlers = instance.GetRequiredServices&lt;IHandler&gt;();
    ///   var results = handlers.Select(h => h.Handle(input)).ToList();
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>An enumerable of all registered implementations</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static IEnumerable<TService> GetRequiredServices<TService>(this object instance) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider is not configured. Call ConfigureServices() first.");
        }

        // Use Microsoft's GetRequiredService explicitly to avoid our extension method
        return ServiceProviderServiceExtensions.GetRequiredService<IEnumerable<TService>>(serviceProvider);
    }

    /// <summary>
    /// Gets all registered keyed implementations of a service type with a specific key.
    /// 
    /// Usage:
    ///   var validators = instance.GetRequiredKeyedServices&lt;IValidator&gt;("user");
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="serviceKey">The key for the keyed services</param>
    /// <returns>An enumerable of all keyed implementations matching the key</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static IEnumerable<TService> GetRequiredKeyedServices<TService>(this object instance, object? serviceKey) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider is not configured. Call ConfigureServices() first.");
        }

        // Use Microsoft's GetRequiredKeyedService explicitly to avoid our extension method
        return ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<IEnumerable<TService>>(serviceProvider, serviceKey);
    }

    /// <summary>
    /// Gets all registered implementations of a service type, or an empty enumerable if none are registered.
    /// 
    /// Usage:
    ///   var handlers = instance.GetServices&lt;IHandler&gt;();
    ///   foreach (var handler in handlers)
    ///   {
    ///       handler.Handle(data);
    ///   }
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>An enumerable of all registered implementations, or empty if none found</returns>
    public static IEnumerable<TService> GetServices<TService>(this object instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        // CRITICAL FIX: Don't intercept IServiceProvider or IServiceScope - let Microsoft's extension methods handle those
        if (instance is IServiceProvider provider)
        {
            return ServiceProviderServiceExtensions.GetServices<TService>(provider);
        }

        if (instance is IServiceScope scope)
        {
            return ServiceProviderServiceExtensions.GetServices<TService>(scope.ServiceProvider);
        }

        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            return [];
        }

        // Use Microsoft's GetServices explicitly to avoid our extension method
        return ServiceProviderServiceExtensions.GetServices<TService>(serviceProvider);
    }

    /// <summary>
    /// Executes a function for each registered implementation of a service type.
    /// Useful for composite or chain-of-responsibility patterns.
    /// 
    /// Usage:
    ///   instance.ForEachService&lt;IMiddleware&gt;(m => m.ProcessRequest(request));
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="action">Action to execute for each service</param>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static void ForEachService<TService>(this object instance, Action<TService> action) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(action);

        var services = instance.GetRequiredServices<TService>();
        foreach (var service in services)
        {
            action(service);
        }
    }

    /// <summary>
    /// Executes an async function for each registered implementation of a service type.
    /// Useful for async chains or workflows.
    /// 
    /// Usage:
    ///   await instance.ForEachServiceAsync&lt;IValidator&gt;(async v => await v.ValidateAsync(model));
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="func">Async function to execute for each service</param>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static async Task ForEachServiceAsync<TService>(this object instance, Func<TService, Task> func) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(func);

        var services = instance.GetRequiredServices<TService>();
        foreach (var service in services)
        {
            await func(service).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Maps a function over all registered implementations and collects results.
    /// 
    /// Usage:
    ///   var results = instance.MapServices&lt;IProcessor, string&gt;(p => p.Process());
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="selector">Function to map each service to a result</param>
    /// <returns>Collection of results from the map function</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static IEnumerable<TResult> MapServices<TService, TResult>(
        this object instance,
        Func<TService, TResult> selector)
        where TService : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(selector);

        var services = instance.GetRequiredServices<TService>();
        return services.Select(selector);
    }

    /// <summary>
    /// Asynchronously maps a function over all registered implementations and collects results.
    /// 
    /// Usage:
    ///   var results = await instance.MapServicesAsync&lt;IQueryHandler, int&gt;(async h => await h.HandleAsync());
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="selector">Async function to map each service to a result</param>
    /// <returns>Collection of results from the async map function</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static async Task<IEnumerable<TResult>> MapServicesAsync<TService, TResult>(
        this object instance,
        Func<TService, Task<TResult>> selector)
        where TService : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(selector);

        var services = instance.GetRequiredServices<TService>();
        var tasks = services.Select(selector).ToList();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results;
    }

    /// <summary>
    /// Gets all services with a predicate filter.
    /// 
    /// Usage:
    ///   var enabledHandlers = instance.GetServices&lt;IHandler&gt;(h => h.IsEnabled);
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Filtered enumerable of services</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static IEnumerable<TService> GetServices<TService>(
        this object instance,
        Func<TService, bool> predicate)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(predicate);

        return instance.GetRequiredServices<TService>().Where(predicate);
    }

    /// <summary>
    /// Gets the first service matching a predicate, or throws if none found.
    /// 
    /// Usage:
    ///   var handler = instance.GetFirstService&lt;IHandler&gt;(h => h.CanHandle(request));
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>The first matching service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or no match found</exception>
    public static TService GetFirstService<TService>(
        this object instance,
        Func<TService, bool> predicate)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(predicate);

        return instance.GetServices(predicate).FirstOrDefault()
            ?? throw new InvalidOperationException($"No service of type {typeof(TService).Name} matches the predicate.");
    }

    /// <summary>
    /// Gets the first service matching a predicate, or null if none found.
    /// 
    /// Usage:
    ///   var handler = instance.GetFirstServiceOrDefault&lt;IHandler&gt;(h => h.CanHandle(request));
    /// </summary>
    /// <typeparam name="TService">The service type to enumerate</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>The first matching service, or null if not found</returns>
    public static TService? GetFirstServiceOrDefault<TService>(
        this object instance,
        Func<TService, bool> predicate)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(predicate);

        return instance.GetServices(predicate).FirstOrDefault();
    }

    /// <summary>
    /// Gets the count of registered implementations of a service type.
    /// 
    /// Usage:
    ///   int count = instance.GetServiceCount&lt;IHandler&gt;();
    /// </summary>
    /// <typeparam name="TService">The service type to count</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>The number of registered implementations</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static int GetServiceCount<TService>(this object instance) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        return instance.GetRequiredServices<TService>().Count();
    }
}
