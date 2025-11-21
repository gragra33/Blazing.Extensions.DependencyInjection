using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for open generic type registration with Blazing.Extensions.DependencyInjection.
/// Provides convenient methods for registering and resolving generic services without explicit closed-type registration.
/// 
/// Open generics are useful for:
/// - Repository patterns: IRepository&lt;T&gt; with Repository&lt;T&gt;
/// - Generic handlers: ICommandHandler&lt;T&gt; with CommandHandler&lt;T&gt;
/// - Validation: IValidator&lt;T&gt; with Validator&lt;T&gt;
/// - Query handlers: IQueryHandler&lt;TQuery, TResult&gt; with QueryHandler&lt;TQuery, TResult&gt;
/// 
/// Usage:
///   services.AddGenericSingleton(typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;));
///   services.AddGenericScoped(typeof(IValidator&lt;&gt;), typeof(Validator&lt;&gt;));
///   
///   var userRepo = provider.GetRequiredService&lt;IRepository&lt;User&gt;&gt;();
///   var productRepo = provider.GetRequiredService&lt;IRepository&lt;Product&gt;&gt;();
/// </summary>
public static class GenericServiceExtensions
{
    /// <summary>
    /// Registers an open generic singleton service.
    /// The same instance will be used for all closed generic types.
    /// 
    /// Usage:
    ///   services.AddGenericSingleton(typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;));
    ///   
    ///   // Then resolve specific instances:
    ///   var userRepo = serviceProvider.GetRequiredService&lt;IRepository&lt;User&gt;&gt;();
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type (e.g., typeof(IRepository&lt;&gt;))</param>
    /// <param name="genericImplementationType">The open generic implementation type (e.g., typeof(Repository&lt;&gt;))</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentException">Thrown if types are not valid generic definitions</exception>
    public static IServiceCollection AddGenericSingleton(
        this IServiceCollection services,
        Type genericInterfaceType,
        Type genericImplementationType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);
        ArgumentNullException.ThrowIfNull(genericImplementationType);

        ValidateGenericTypes(genericInterfaceType, genericImplementationType);

        services.AddSingleton(genericInterfaceType, genericImplementationType);
        return services;
    }

    /// <summary>
    /// Registers an open generic transient service.
    /// A new instance will be created for each request.
    /// 
    /// Usage:
    ///   services.AddGenericTransient(typeof(ICommandHandler&lt;&gt;), typeof(CommandHandler&lt;&gt;));
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type</param>
    /// <param name="genericImplementationType">The open generic implementation type</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentException">Thrown if types are not valid generic definitions</exception>
    public static IServiceCollection AddGenericTransient(
        this IServiceCollection services,
        Type genericInterfaceType,
        Type genericImplementationType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);
        ArgumentNullException.ThrowIfNull(genericImplementationType);

        ValidateGenericTypes(genericInterfaceType, genericImplementationType);

        services.AddTransient(genericInterfaceType, genericImplementationType);
        return services;
    }

    /// <summary>
    /// Registers an open generic scoped service.
    /// A new instance will be created per scope.
    /// 
    /// Usage:
    ///   services.AddGenericScoped(typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;));
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type</param>
    /// <param name="genericImplementationType">The open generic implementation type</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentException">Thrown if types are not valid generic definitions</exception>
    public static IServiceCollection AddGenericScoped(
        this IServiceCollection services,
        Type genericInterfaceType,
        Type genericImplementationType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);
        ArgumentNullException.ThrowIfNull(genericImplementationType);

        ValidateGenericTypes(genericInterfaceType, genericImplementationType);

        services.AddScoped(genericInterfaceType, genericImplementationType);
        return services;
    }

    /// <summary>
    /// Registers multiple open generic services with a specific lifetime.
    /// Useful for batch registering generic service families.
    /// 
    /// Usage:
    ///   var genericServices = new[]
    ///   {
    ///       (typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;)),
    ///       (typeof(IValidator&lt;&gt;), typeof(Validator&lt;&gt;))
    ///   };
    ///   services.AddGenericServices(ServiceLifetime.Scoped, genericServices);
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="lifetime">The service lifetime</param>
    /// <param name="genericServicePairs">Array of (interface, implementation) type pairs</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGenericServices(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params (Type InterfaceType, Type ImplementationType)[] genericServicePairs)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericServicePairs);

        foreach (var (interfaceType, implementationType) in genericServicePairs)
        {
            ValidateGenericTypes(interfaceType, implementationType);

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(interfaceType, implementationType);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(interfaceType, implementationType);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(interfaceType, implementationType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime");
            }
        }

        return services;
    }

    /// <summary>
    /// Registers an open generic service with its implementation type derived from the interface.
    /// Assumes the implementation type has the same name as the interface (without the 'I' prefix).
    /// 
    /// Example: IRepository&lt;&gt; -> Repository&lt;&gt;
    /// 
    /// Usage:
    ///   services.AddGenericSingleton(typeof(IRepository&lt;&gt;));
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown if implementation type cannot be automatically determined</exception>
    public static IServiceCollection AddGenericSingleton(
        this IServiceCollection services,
        Type genericInterfaceType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);

        var implementationType = DeriveImplementationType(genericInterfaceType);
        return services.AddGenericSingleton(genericInterfaceType, implementationType);
    }

    /// <summary>
    /// Registers an open generic transient service with derived implementation type.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown if implementation type cannot be automatically determined</exception>
    public static IServiceCollection AddGenericTransient(
        this IServiceCollection services,
        Type genericInterfaceType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);

        var implementationType = DeriveImplementationType(genericInterfaceType);
        return services.AddGenericTransient(genericInterfaceType, implementationType);
    }

    /// <summary>
    /// Registers an open generic scoped service with derived implementation type.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="genericInterfaceType">The open generic interface type</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown if implementation type cannot be automatically determined</exception>
    public static IServiceCollection AddGenericScoped(
        this IServiceCollection services,
        Type genericInterfaceType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);

        var implementationType = DeriveImplementationType(genericInterfaceType);
        return services.AddGenericScoped(genericInterfaceType, implementationType);
    }

    /// <summary>
    /// Validates that a type pair represents valid open generic types for registration.
    /// </summary>
    /// <param name="interfaceType">The interface type to validate</param>
    /// <param name="implementationType">The implementation type to validate</param>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    private static void ValidateGenericTypes(Type interfaceType, Type implementationType)
    {
        if (!interfaceType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"Interface type '{interfaceType.Name}' must be a generic type definition (e.g., IRepository&lt;&gt;)");
        }

        if (!implementationType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"Implementation type '{implementationType.Name}' must be a generic type definition (e.g., Repository&lt;&gt;)");
        }

        var interfaceGenericArgs = interfaceType.GetGenericArguments();
        var implementationGenericArgs = implementationType.GetGenericArguments();

        if (interfaceGenericArgs.Length != implementationGenericArgs.Length)
        {
            throw new ArgumentException(
                $"Generic argument count mismatch: {interfaceType.Name} has {interfaceGenericArgs.Length} arguments, " +
                $"but {implementationType.Name} has {implementationGenericArgs.Length} arguments");
        }
    }

    /// <summary>
    /// Derives an implementation type from an interface type by removing the 'I' prefix.
    /// For example: IRepository&lt;&gt; -> Repository&lt;&gt;
    /// </summary>
    /// <param name="interfaceType">The interface type</param>
    /// <returns>The derived implementation type</returns>
    /// <exception cref="InvalidOperationException">Thrown if implementation type cannot be found</exception>
    private static Type DeriveImplementationType(Type interfaceType)
    {
        if (!interfaceType.IsInterface || !interfaceType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException(
                $"Cannot automatically derive implementation type from '{interfaceType.Name}'. " +
                "Please provide both interface and implementation types explicitly.");
        }

        var interfaceName = interfaceType.Name;
        if (!interfaceName.StartsWith('I') || interfaceName.Length < 2)
        {
            throw new InvalidOperationException(
                $"Cannot derive implementation type from '{interfaceName}': interface name must start with 'I'");
        }

        var implementationTypeName = interfaceName.Substring(1); // Remove 'I' prefix
        var implementationType = interfaceType.Assembly.GetType(
            interfaceType.Namespace + "." + implementationTypeName,
            throwOnError: false);

        if (implementationType == null)
        {
            throw new InvalidOperationException(
                $"Could not find implementation type '{implementationTypeName}' in namespace '{interfaceType.Namespace}'. " +
                "Please register the generic service with both interface and implementation types explicitly.");
        }

        return implementationType;
    }
}
