using System.Reflection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for service decoration/interception.
/// Provides convenient methods for wrapping services with decorators for cross-cutting concerns.
/// 
/// Service decoration is useful for:
/// - Adding caching to services
/// - Logging/monitoring call traces
/// - Validation wrappers for input/output
/// - Performance optimization
/// - Cross-cutting concerns
/// 
/// Usage:
///   services.AddSingleton&lt;IRepository, SqlRepository&gt;();
///   services.Decorate&lt;IRepository&gt;((inner, provider) => 
///       new CachedRepository(inner));
///   services.AddCachingDecorator&lt;IUserService&gt;();
///   services.AddLoggingDecorator&lt;IOrderService&gt;();
/// </summary>
public static class ServiceDecorationExtensions
{
    /// <summary>
    /// Decorates a singleton service with a decorator factory.
    /// The decorator receives the inner service and service provider.
    /// 
    /// Usage:
    ///   services.AddSingleton&lt;IRepository, SqlRepository&gt;();
    ///   services.Decorate&lt;IRepository&gt;((inner, provider) => 
    ///       new CachedRepository(inner));
    /// </summary>
    /// <typeparam name="TService">The service type to decorate</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="decoratorFactory">Factory that creates the decorated service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decoratorFactory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(decoratorFactory);

        // Find and remove existing registration
        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered");

        var objectFactory = ActivatorUtilities.CreateFactory(
            wrappedDescriptor.ImplementationType ?? typeof(TService), 
            Type.EmptyTypes);

        services.Remove(wrappedDescriptor);

        // Register decorated version
        services.AddSingleton(provider =>
        {
            var inner = (TService)objectFactory(provider, null);
            return decoratorFactory(inner, provider);
        });

        return services;
    }

    /// <summary>
    /// Adds a caching decorator to a service.
    /// Results are cached in memory with configurable duration.
    /// 
    /// Usage:
    ///   services.AddSingleton&lt;IUserService, UserService&gt;();
    ///   services.AddCachingDecorator&lt;IUserService&gt;();
    /// </summary>
    /// <typeparam name="TService">The service type to decorate</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="cacheDurationSeconds">How long to cache results (default 300 seconds)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCachingDecorator<TService>(
        this IServiceCollection services,
        int cacheDurationSeconds = 300)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);

        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered");

        var objectFactory = ActivatorUtilities.CreateFactory(
            wrappedDescriptor.ImplementationType ?? typeof(TService),
            Type.EmptyTypes);

        services.Remove(wrappedDescriptor);

        services.AddSingleton(provider =>
        {
            var inner = (TService)objectFactory(provider, null);
            return CachingDecorator<TService>.Create(inner, cacheDurationSeconds);
        });

        return services;
    }

    /// <summary>
    /// Adds a logging decorator to a service.
    /// Logs method calls, arguments, and execution time.
    /// 
    /// Usage:
    ///   services.AddSingleton&lt;IOrderService, OrderService&gt;();
    ///   services.AddLoggingDecorator&lt;IOrderService&gt;();
    /// </summary>
    /// <typeparam name="TService">The service type to decorate</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLoggingDecorator<TService>(
        this IServiceCollection services)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);

        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered");

        var objectFactory = ActivatorUtilities.CreateFactory(
            wrappedDescriptor.ImplementationType ?? typeof(TService),
            Type.EmptyTypes);

        services.Remove(wrappedDescriptor);

        services.AddSingleton(provider =>
        {
            var inner = (TService)objectFactory(provider, null);
            return LoggingDecorator<TService>.Create(inner);
        });

        return services;
    }
}

/// <summary>
/// Caching decorator that caches method results.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - used via DispatchProxy.Create
internal sealed class CachingDecorator<T> : DispatchProxy where T : class
{
    private T _inner = null!;
    private int _cacheDurationSeconds;
    private readonly Dictionary<string, (object? Value, DateTime Expiry)> _cache = new();

    /// <summary>
    /// Creates a new caching decorator instance.
    /// </summary>
    public static T Create(T inner, int cacheDurationSeconds = 300)
    {
        var proxy = DispatchProxy.Create<T, CachingDecorator<T>>();
        var decorator = (CachingDecorator<T>)(object)proxy;
        decorator._inner = inner;
        decorator._cacheDurationSeconds = cacheDurationSeconds;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
            return null;

        var argHashes = args == null ? "null" : string.Join("_", args.Select(a => a?.GetHashCode() ?? 0));
        var cacheKey = $"{targetMethod.Name}_{argHashes}";

        lock (_cache)
        {
            if (_cache.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow < cached.Expiry)
                return cached.Value;

            var result = targetMethod.Invoke(_inner, args);
            _cache[cacheKey] = (result, DateTime.UtcNow.AddSeconds(_cacheDurationSeconds));
            return result;
        }
    }
}
#pragma warning restore CA1812

/// <summary>
/// Logging decorator that logs method calls and execution time.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - used via DispatchProxy.Create
internal sealed class LoggingDecorator<T> : DispatchProxy where T : class
{
    private T _inner = null!;

    /// <summary>
    /// Creates a new logging decorator instance.
    /// </summary>
    public static T Create(T inner)
    {
        var proxy = DispatchProxy.Create<T, LoggingDecorator<T>>();
        var decorator = (LoggingDecorator<T>)(object)proxy;
        decorator._inner = inner;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
            return null;

        var startTime = DateTime.UtcNow;
        try
        {
            var argString = args == null ? "" : string.Join(", ", args.Select(a => a?.ToString() ?? "null"));
            System.Diagnostics.Debug.WriteLine($"[{typeof(T).Name}] Calling {targetMethod.Name}({argString})");

            var result = targetMethod.Invoke(_inner, args);

            var duration = DateTime.UtcNow - startTime;
            System.Diagnostics.Debug.WriteLine($"[{typeof(T).Name}] {targetMethod.Name} completed in {duration.TotalMilliseconds}ms");

            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            System.Diagnostics.Debug.WriteLine($"[{typeof(T).Name}] {targetMethod.Name} failed after {duration.TotalMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}
#pragma warning restore CA1812
