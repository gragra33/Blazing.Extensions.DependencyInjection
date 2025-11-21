namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates service scoping and lifecycle management including synchronous and asynchronous disposal patterns.
/// Shows how to create scopes, use scoped services, and manage resources properly.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class ServiceScopingExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Service Scoping & Lifecycle Management";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceScopingExample"/> class.
    /// </summary>
    public ServiceScopingExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateSynchronousScope();
        DemonstrateAsyncScope();
        DemonstrateScopedServiceMethod();
        DemonstrateScopedKeyedService();
    }

    /// <summary>
    /// Configures services for scoping demonstrations.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            services.AddScoped<IScopedService, ScopedService>();
            services.AddKeyedScoped<IScopedService, ScopedService>("primary");
            services.AddSingleton<ILoggingService, ConsoleLoggingService>();
        });
    }

    /// <summary>
    /// Demonstrates creating and using a synchronous service scope.
    /// </summary>
    private void DemonstrateSynchronousScope()
    {
        Console.WriteLine("  Creating synchronous scope...");

        using var scope = _host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IScopedService>();
        var scopeId = service.GetScopeId();

        Console.WriteLine($"    + Scoped service created with ID: {scopeId}");
        Console.WriteLine($"    + Scope automatically disposed");
    }

    /// <summary>
    /// Demonstrates creating and using an asynchronous service scope.
    /// </summary>
    private void DemonstrateAsyncScope()
    {
        Console.WriteLine("  Creating async scope...");

        Task.Run(async () =>
        {
            await using var scope = _host.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IScopedService>();
            var scopeId = service.GetScopeId();

            Console.WriteLine($"    + Async scoped service created with ID: {scopeId}");
            Console.WriteLine($"    + Async scope automatically disposed");
        }).Wait();
    }

    /// <summary>
    /// Demonstrates using convenience methods for scoped service execution.
    /// </summary>
    private void DemonstrateScopedServiceMethod()
    {
        Console.WriteLine("  Using GetScopedService convenience method...");

        _host.GetScopedService<IScopedService>(service =>
        {
            var scopeId = service.GetScopeId();
            Console.WriteLine($"    + Scoped service executed with ID: {scopeId}");
            Console.WriteLine($"    + Scope created and disposed automatically");
        });
    }

    /// <summary>
    /// Demonstrates using keyed services within a scope.
    /// </summary>
    private void DemonstrateScopedKeyedService()
    {
        Console.WriteLine("  Using GetScopedKeyedService...");

        _host.GetScopedKeyedService<IScopedService>("primary", service =>
        {
            var scopeId = service.GetScopeId();
            Console.WriteLine($"    + Keyed scoped service executed with ID: {scopeId}");
            Console.WriteLine($"    + Keyed scope created and disposed automatically");
        });
    }
}

/// <summary>
/// Represents a scoped service for demonstration purposes.
/// </summary>
public interface IScopedService
{
    /// <summary>
    /// Gets the unique identifier for this scope instance.
    /// </summary>
    /// <returns>A unique GUID for this scope instance.</returns>
    string GetScopeId();
}

/// <summary>
/// Implementation of <see cref="IScopedService"/> that generates a unique ID per scope.
/// </summary>
public class ScopedService : IScopedService
{
    private readonly string _scopeId = Guid.NewGuid().ToString("N")[..8];

    /// <inheritdoc/>
    public string GetScopeId() => _scopeId;
}
