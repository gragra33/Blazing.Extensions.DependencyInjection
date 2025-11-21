namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for service factory registration with Blazing.Extensions.DependencyInjection.
/// Provides convenient methods for registering services using factory delegates.
/// 
/// Factory patterns are useful for:
/// - Complex conditional service creation
/// - Services that depend on configuration
/// - Runtime service composition
/// - Wrapping or decorating services
/// 
/// Usage:
///   services.RegisterFactory&lt;IService&gt;(provider => 
///       new Service(provider.GetRequiredService&lt;IConfiguration&gt;()));
/// </summary>
public static class ServiceFactoryExtensions
{
    /// <summary>
    /// Registers a singleton service using a factory function with IServiceProvider parameter.
    /// The factory receives the service provider and must return a configured instance.
    /// 
    /// Usage:
    ///   services.RegisterFactory&lt;IService&gt;(provider => 
    ///       new Service(provider.GetRequiredService&lt;IDependency&gt;()));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Registers a transient service using a factory function.
    /// A new instance is created on each request.
    /// 
    /// Usage:
    ///   services.RegisterTransientFactory&lt;IService&gt;(provider => new Service(...));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterTransientFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddTransient(factory);
        return services;
    }

    /// <summary>
    /// Registers a scoped service using a factory function.
    /// A new instance is created per scope.
    /// 
    /// Usage:
    ///   services.RegisterScopedFactory&lt;IService&gt;(provider => new Service(...));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterScopedFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddScoped(factory);
        return services;
    }

    /// <summary>
    /// Registers a keyed singleton service using a factory function.
    /// The factory receives both the provider and the service key.
    /// 
    /// Usage:
    ///   services.RegisterKeyedFactory&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterKeyedFactory<TService>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedSingleton(serviceKey, factory);
        return services;
    }

    /// <summary>
    /// Registers a keyed transient service using a factory function.
    /// 
    /// Usage:
    ///   services.RegisterKeyedTransientFactory&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterKeyedTransientFactory<TService>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedTransient(serviceKey, factory);
        return services;
    }

    /// <summary>
    /// Registers a keyed scoped service using a factory function.
    /// 
    /// Usage:
    ///   services.RegisterKeyedScopedFactory&lt;IService&gt;("key", (provider, key) => new Service(...));
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterKeyedScopedFactory<TService>(
        this IServiceCollection services,
        object? serviceKey,
        Func<IServiceProvider, object?, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddKeyedScoped(serviceKey, factory);
        return services;
    }

    /// <summary>
    /// Registers a service with conditional factory logic based on configuration.
    /// Useful for environment-specific service registration.
    /// 
    /// Usage:
    ///   services.RegisterConditionalFactory&lt;IService&gt;(provider =>
    ///   {
    ///       var env = provider.GetRequiredService&lt;IHostEnvironment&gt;();
    ///       return env.IsProduction() ? new ProdService() : new DevService();
    ///   });
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Conditional factory function</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection RegisterConditionalFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Registers a singleton service instance created via factory function.
    /// The factory is called once and the result is cached.
    /// 
    /// Usage:
    ///   var config = services.RegisterSingletonFactory(provider => LoadConfiguration());
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that creates the service</param>
    /// <returns>The created service instance</returns>
    public static TService RegisterSingletonFactory<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        var provider = services.BuildServiceProvider();
        var instance = factory(provider);
        services.AddSingleton(instance);
        return instance;
    }
}
