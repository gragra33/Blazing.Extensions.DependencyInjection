namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Interface for services that require async initialization during application startup.
/// Implement this interface for services that need async setup before the application runs.
/// </summary>
public interface IAsyncInitializable
{
    /// <summary>
    /// Called during application startup to initialize the service asynchronously.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    Task InitializeAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Priority for initialization ordering. Higher values initialize first. Default is 0.
    /// </summary>
    int InitializationPriority => 0;

    /// <summary>
    /// Optional list of service types that must be initialized before this one.
    /// </summary>
    IEnumerable<Type>? DependsOn => null;
}

/// <summary>
/// Extension methods for async service initialization.
/// Enables declarative startup initialization patterns with dependency ordering.
/// </summary>
public static class AsyncInitializationExtensions
{
    /// <summary>
    /// Initializes a single registered service that implements IAsyncInitializable.
    /// Automatically resolves and initializes dependencies first.
    /// </summary>
    /// <typeparam name="TService">The service to initialize</typeparam>
    /// <param name="serviceProvider">The service provider</param>
    public static async Task InitializeAsync<TService>(
        this IServiceProvider serviceProvider)
        where TService : IAsyncInitializable
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var service = serviceProvider.GetRequiredService<TService>();
        await service.InitializeAsync(serviceProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes all registered implementations of IAsyncInitializable.
    /// Automatically handles dependency ordering based on InitializationPriority and DependsOn.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public static async Task InitializeAllAsync(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var allServices = serviceProvider.GetServices<IAsyncInitializable>().ToList();
        var initialized = new HashSet<Type>();
        var initializing = new HashSet<Type>();

        async Task InitializeService(IAsyncInitializable service)
        {
            var serviceType = service.GetType();

            if (initialized.Contains(serviceType))
                return;

            if (initializing.Contains(serviceType))
                throw new InvalidOperationException($"Circular dependency detected in {serviceType.Name}");

            initializing.Add(serviceType);

            // Initialize dependencies first
            if (service.DependsOn != null)
            {
                foreach (var depType in service.DependsOn)
                {
                    var depService = allServices.FirstOrDefault(s => s.GetType() == depType);
                    if (depService != null)
                        await InitializeService(depService).ConfigureAwait(false);
                }
            }

            await service.InitializeAsync(serviceProvider).ConfigureAwait(false);
            initialized.Add(serviceType);
            initializing.Remove(serviceType);
        }

        // Sort by priority (higher first) then by dependencies
        var sortedServices = allServices
            .OrderByDescending(s => s.InitializationPriority)
            .ToList();

        foreach (var service in sortedServices)
        {
            await InitializeService(service).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Initializes all implementations of a specific async initializable interface.
    /// </summary>
    /// <typeparam name="TInterface">The service interface to initialize</typeparam>
    /// <param name="serviceProvider">The service provider</param>
    public static async Task InitializeAllAsync<TInterface>(
        this IServiceProvider serviceProvider)
        where TInterface : class, IAsyncInitializable
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var services = serviceProvider.GetServices<TInterface>().Cast<IAsyncInitializable>().ToList();
        var initialized = new HashSet<Type>();

        async Task InitializeService(IAsyncInitializable service)
        {
            var serviceType = service.GetType();

            if (initialized.Contains(serviceType))
                return;

            // Initialize dependencies first
            if (service.DependsOn != null)
            {
                foreach (var depType in service.DependsOn)
                {
                    var depService = services.FirstOrDefault(s => s.GetType() == depType);
                    if (depService != null)
                        await InitializeService(depService).ConfigureAwait(false);
                }
            }

            await service.InitializeAsync(serviceProvider).ConfigureAwait(false);
            initialized.Add(serviceType);
        }

        // Sort by priority (higher first)
        var sortedServices = services
            .OrderByDescending(s => s.InitializationPriority)
            .ToList();

        foreach (var service in sortedServices)
        {
            await InitializeService(service).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the initialization order for all IAsyncInitializable services.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>Initialization order information</returns>
    public static InitializationOrder GetInitializationOrder(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var allServices = serviceProvider.GetServices<IAsyncInitializable>().ToList();
        var order = new InitializationOrder();

        var sorted = allServices
            .OrderByDescending(s => s.InitializationPriority)
            .ThenBy(s => s.GetType().Name)
            .ToList();

        var stepOrder = 0;
        foreach (var service in sorted)
        {
            var step = new InitializationStep
            {
                Order = stepOrder++,
                ServiceType = service.GetType(),
                Priority = service.InitializationPriority
            };

            if (service.DependsOn != null)
            {
                foreach (var dep in service.DependsOn)
                {
                    ((System.Collections.ObjectModel.Collection<Type>)step.DependsOn).Add(dep);
                }
            }

            ((System.Collections.ObjectModel.Collection<InitializationStep>)order.Steps).Add(step);
        }

        return order;
    }

    /// <summary>
    /// Registers a startup action that executes during initialization.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="action">The async startup action</param>
    /// <param name="priority">Priority for execution (higher first, default 0)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStartupAction(
        this IServiceCollection services,
        Func<IServiceProvider, Task> action,
        int priority = 0)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(action);

        services.AddSingleton<IAsyncInitializable>(new StartupAction(action, priority));
        return services;
    }
}

/// <summary>
/// Represents the initialization order of async initializable services.
/// </summary>
public class InitializationOrder
{
    /// <summary>
    /// The ordered list of initialization steps.
    /// </summary>
    public ICollection<InitializationStep> Steps { get; } = new System.Collections.ObjectModel.Collection<InitializationStep>();
}

/// <summary>
/// Represents a single initialization step.
/// </summary>
public class InitializationStep
{
    /// <summary>
    /// The order in which this service will be initialized.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The service type being initialized.
    /// </summary>
    public Type ServiceType { get; set; } = null!;

    /// <summary>
    /// The initialization priority of this service.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Service types that must be initialized before this one.
    /// </summary>
    public ICollection<Type> DependsOn { get; } = new System.Collections.ObjectModel.Collection<Type>();
}

/// <summary>
/// Internal implementation of IAsyncInitializable for startup actions.
/// </summary>
internal sealed class StartupAction : IAsyncInitializable
{
    private readonly Func<IServiceProvider, Task> _action;
    private readonly int _priority;

    public int InitializationPriority => _priority;
    public IEnumerable<Type>? DependsOn => null;

    public StartupAction(Func<IServiceProvider, Task> action, int priority)
    {
        _action = action;
        _priority = priority;
    }

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        return _action(serviceProvider);
    }
}
