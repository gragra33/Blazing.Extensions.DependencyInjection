using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for lazy service initialization with Blazing.Extensions.DependencyInjection.
/// Provides convenient methods for registering and resolving services with lazy initialization.
/// 
/// Lazy initialization is useful for:
/// - Expensive-to-create services that may not be used
/// - Services that need to be initialized on first use
/// - Breaking circular dependency chains
/// - Performance optimization in complex scenarios
/// 
/// Usage:
///   services.AddLazySingleton&lt;IExpensiveService, ExpensiveService&gt;();
///   
///   var lazyService = instance.GetRequiredService&lt;Lazy&lt;IExpensiveService&gt;&gt;();
///   var service = lazyService.Value; // Created on first access
/// </summary>
public static class LazyServiceExtensions
{
    /// <summary>
    /// Registers a singleton service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// The service is created only when first accessed via the Lazy&lt;T&gt;.Value property.
    /// 
    /// Usage:
    ///   services.AddLazySingleton&lt;IService, Service&gt;();
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazySingleton<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(provider =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed singleton service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedSingleton&lt;IService, Service&gt;("key");
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedSingleton<TInterface, TImplementation>(
        this IServiceCollection services, 
        object? serviceKey)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKeyedSingleton(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a transient service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// Each access to Lazy&lt;T&gt;.Value creates a new instance.
    /// 
    /// Usage:
    ///   services.AddLazyTransient&lt;IService, Service&gt;();
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyTransient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(provider =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed transient service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedTransient&lt;IService, Service&gt;("key");
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedTransient<TInterface, TImplementation>(
        this IServiceCollection services,
        object? serviceKey)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKeyedTransient(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a scoped service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// The service is created once per scope on first access.
    /// 
    /// Usage:
    ///   services.AddLazyScoped&lt;IService, Service&gt;();
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyScoped<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped(provider =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed scoped service wrapped in Lazy&lt;T&gt; for lazy initialization.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedScoped&lt;IService, Service&gt;("key");
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <typeparam name="TImplementation">The service implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedScoped<TInterface, TImplementation>(
        this IServiceCollection services,
        object? serviceKey)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKeyedScoped(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => ActivatorUtilities.CreateInstance<TImplementation>(provider)));

        return services;
    }

    /// <summary>
    /// Registers a singleton service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazySingleton&lt;IService&gt;(provider => new Service(provider.GetRequiredService&lt;IDependency&gt;()));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazySingleton<TInterface>(
        this IServiceCollection services,
        Func<IServiceProvider, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton(provider => new Lazy<TInterface>(() => factory(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed singleton service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedSingleton&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedSingleton<TInterface>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedSingleton(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => factory(provider, key)));

        return services;
    }

    /// <summary>
    /// Registers a transient service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazyTransient&lt;IService&gt;(provider => new Service(provider.GetRequiredService&lt;IDependency&gt;()));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyTransient<TInterface>(
        this IServiceCollection services,
        Func<IServiceProvider, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddTransient(provider => new Lazy<TInterface>(() => factory(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed transient service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedTransient&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedTransient<TInterface>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedTransient(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => factory(provider, key)));

        return services;
    }

    /// <summary>
    /// Registers a scoped service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazyScoped&lt;IService&gt;(provider => new Service(provider.GetRequiredService&lt;IDependency&gt;()));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyScoped<TInterface>(
        this IServiceCollection services,
        Func<IServiceProvider, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddScoped(provider => new Lazy<TInterface>(() => factory(provider)));

        return services;
    }

    /// <summary>
    /// Registers a keyed scoped service wrapped in Lazy&lt;T&gt; using a factory function.
    /// 
    /// Usage:
    ///   services.AddLazyKeyedScoped&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TInterface">The service interface type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function to create the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLazyKeyedScoped<TInterface>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedScoped(serviceKey, (provider, key) =>
            new Lazy<TInterface>(() => factory(provider, key)));

        return services;
    }

    /// <summary>
    /// Gets a lazy-initialized service from the instance's service provider.
    /// The service is created only when accessed.
    /// 
    /// Usage:
    ///   var lazyService = instance.GetLazyService&lt;IService&gt;();
    ///   var service = lazyService.Value; // Created on first access
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>A Lazy&lt;TService&gt; wrapper around the service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not registered</exception>
    public static Lazy<TService> GetLazyService<TService>(this object instance) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        return instance.GetRequiredService<Lazy<TService>>();
    }

    /// <summary>
    /// Gets a lazy-initialized keyed service from the instance's service provider.
    /// 
    /// Usage:
    ///   var lazyService = instance.GetLazyKeyedService&lt;IService&gt;("key");
    ///   var service = lazyService.Value; // Created on first access
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <returns>A Lazy&lt;TService&gt; wrapper around the service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not registered</exception>
    public static Lazy<TService> GetLazyKeyedService<TService>(this object instance, object? serviceKey) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        return instance.GetRequiredKeyedService<Lazy<TService>>(serviceKey);
    }
}
