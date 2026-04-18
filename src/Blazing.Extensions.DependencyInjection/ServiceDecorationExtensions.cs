namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for service decoration/interception.
/// Provides a factory-based decorator pattern for wrapping registered services with cross-cutting concerns.
///
/// Usage:
///   services.AddSingleton&lt;IRepository, SqlRepository&gt;();
///   services.Decorate&lt;IRepository&gt;((inner, provider) =&gt;
///       new CachedRepository(inner));
/// </summary>
public static class ServiceDecorationExtensions
{
    /// <summary>
    /// Decorates an existing service registration with a decorator factory.
    /// The decorator receives the inner service and service provider, and must return
    /// the same service type.
    ///
    /// Usage:
    ///   services.AddSingleton&lt;IRepository, SqlRepository&gt;();
    ///   services.Decorate&lt;IRepository&gt;((inner, provider) =&gt;
    ///       new CachedRepository(inner));
    /// </summary>
    /// <typeparam name="TService">The service type to decorate.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="decoratorFactory">Factory that receives the inner service and creates the decorator.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="TService"/> is not registered in the collection.
    /// </exception>
    public static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decoratorFactory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(decoratorFactory);

        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered");

        var objectFactory = ActivatorUtilities.CreateFactory(
            wrappedDescriptor.ImplementationType ?? typeof(TService),
            Type.EmptyTypes);

        var lifetime = wrappedDescriptor.Lifetime;
        services.Remove(wrappedDescriptor);

        var descriptor = ServiceDescriptor.Describe(
            typeof(TService),
            provider =>
            {
                var inner = (TService)objectFactory(provider, null);
                return decoratorFactory(inner, provider);
            },
            lifetime);
        services.Add(descriptor);

        return services;
    }
}
