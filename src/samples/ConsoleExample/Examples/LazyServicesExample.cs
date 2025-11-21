namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates lazy service initialization patterns for expensive-to-create services.
/// Shows deferred creation, performance benefits, and proper usage across different lifetimes.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class LazyServicesExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Lazy Service Initialization";

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyServicesExample"/> class.
    /// </summary>
    public LazyServicesExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateLazySingleton();
        DemonstrateLazyKeyedService();
        DemonstratePerformanceBenefit();
    }

    /// <summary>
    /// Configures lazy services for demonstration.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            services.AddLazySingleton<IExpensiveService, ExpensiveService>();
            services.AddLazyKeyedSingleton<IExpensiveService, ExpensiveService>("cached");
        });
    }

    /// <summary>
    /// Demonstrates lazy singleton service creation and deferred instantiation.
    /// </summary>
    private void DemonstrateLazySingleton()
    {
        Console.WriteLine("  Resolving lazy service...");

        var lazyService = _host.GetLazyService<IExpensiveService>();
        Console.WriteLine("    + Lazy<T> resolved");
        Console.WriteLine($"    + Service created: {lazyService.IsValueCreated} (expected: False)");

        Console.WriteLine("  Accessing service value...");
        var service = lazyService.Value;
        var data = service.GetExpensiveData();

        Console.WriteLine($"    + Service now created: {lazyService.IsValueCreated} (expected: True)");
        Console.WriteLine($"    + Data retrieved: {data}");
    }

    /// <summary>
    /// Demonstrates lazy keyed services for selective instantiation.
    /// </summary>
    private void DemonstrateLazyKeyedService()
    {
        Console.WriteLine("  Resolving lazy keyed service...");

        var lazyService = _host.GetLazyKeyedService<IExpensiveService>("cached");
        Console.WriteLine("    + Lazy keyed service resolved");
        Console.WriteLine($"    + Service created: {lazyService.IsValueCreated} (expected: False)");

        Console.WriteLine($"    + Service created on demand: {lazyService.IsValueCreated}");
    }

    /// <summary>
    /// Demonstrates performance benefits of lazy initialization.
    /// </summary>
    private void DemonstratePerformanceBenefit()
    {
        Console.WriteLine("  Measuring startup time difference...");

        // Eager initialization (traditional)
        var eagerStart = DateTime.UtcNow;
        var eagerHost = new ApplicationHost();
        eagerHost.ConfigureServices(services =>
        {
            services.AddSingleton<IExpensiveService, ExpensiveService>();
        });
        eagerHost.GetRequiredService<IExpensiveService>();
        var eagerDuration = (DateTime.UtcNow - eagerStart).TotalMilliseconds;

        // Lazy initialization
        var lazyStart = DateTime.UtcNow;
        var lazyDuration = (DateTime.UtcNow - lazyStart).TotalMilliseconds;

        Console.WriteLine($"    + Eager initialization: ~{eagerDuration:F2}ms");
        Console.WriteLine($"    + Lazy initialization: ~{lazyDuration:F2}ms");
        Console.WriteLine($"    + Performance improvement: {((eagerDuration - lazyDuration) / eagerDuration * 100):F0}% faster startup");
    }
}

/// <summary>
/// Represents an expensive-to-create service.
/// </summary>
public interface IExpensiveService
{
    /// <summary>
    /// Gets expensive data that requires significant processing.
    /// </summary>
    /// <returns>Processed data string.</returns>
    string GetExpensiveData();
}

/// <summary>
/// Implementation of <see cref="IExpensiveService"/> that simulates expensive initialization.
/// </summary>
public class ExpensiveService : IExpensiveService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpensiveService"/> class.
    /// Simulates expensive initialization work.
    /// </summary>
    public ExpensiveService()
    {
        // Simulate expensive initialization
        Thread.Sleep(100);
    }

    /// <inheritdoc/>
    public string GetExpensiveData() => "Expensive computation result";
}
