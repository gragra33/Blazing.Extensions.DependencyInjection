namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Marks a class for automatic dependency injection registration.
/// Classes decorated with this attribute will be automatically discovered and registered
/// when using the Register() method without generic type parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AutoRegisterAttribute : Attribute
{
    /// <summary>
    /// Gets the service lifetime for the registered service.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Gets the interface type to register the service as.
    /// If null, the service will be registered as its own type and any implemented interfaces.
    /// </summary>
    public Type? ServiceType { get; }

    /// <summary>
    /// Initializes a new instance of the AutoRegisterAttribute with the default Transient lifetime.
    /// </summary>
    public AutoRegisterAttribute() : this(ServiceLifetime.Transient)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AutoRegisterAttribute with the specified lifetime.
    /// </summary>
    /// <param name="lifetime">The service lifetime</param>
    public AutoRegisterAttribute(ServiceLifetime lifetime) : this(lifetime, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AutoRegisterAttribute with the specified lifetime and service type.
    /// </summary>
    /// <param name="lifetime">The service lifetime</param>
    /// <param name="serviceType">The interface type to register the service as</param>
    public AutoRegisterAttribute(ServiceLifetime lifetime, Type? serviceType)
    {
        Lifetime = lifetime;
        ServiceType = serviceType;
    }
}