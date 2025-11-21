namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates async service initialization with priority-based ordering and dependency management.
/// Shows the IAsyncInitializable interface for startup initialization patterns.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class AsyncInitializationExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Async Service Initialization";

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncInitializationExample"/> class.
    /// </summary>
    public AsyncInitializationExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateAsyncInitialization();
        DemonstrateInitializationOrder();
        DemonstrateStartupActions();
    }

    /// <summary>
    /// Configures async initializable services.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            services.AddSingleton<IAsyncInitializable, DatabaseInitializer>();
            services.AddSingleton<IAsyncInitializable, CacheInitializer>();
            services.AddSingleton<IAsyncInitializable, IndexInitializer>();

            services.AddStartupAction(async provider =>
            {
                Console.WriteLine("      ? Startup action: Application ready");
                await Task.CompletedTask;
            }, priority: 0);
        });
    }

    /// <summary>
    /// Demonstrates async initialization of all services.
    /// </summary>
    private void DemonstrateAsyncInitialization()
    {
        Console.WriteLine("  Initializing all async services...");

        Task.Run(async () =>
        {
            await _host.GetServices()!.InitializeAllAsync();
            Console.WriteLine($"    + All services initialized successfully");
        }).Wait();
    }

    /// <summary>
    /// Demonstrates initialization order based on priority and dependencies.
    /// </summary>
    private void DemonstrateInitializationOrder()
    {
        Console.WriteLine("  Getting initialization order...");

        var order = _host.GetServices()!.GetInitializationOrder();

        Console.WriteLine($"    + Initialization steps: {order.Steps.Count}");
        foreach (var step in order.Steps)
        {
            Console.WriteLine($"      -> Step {step.Order}: {step.ServiceType.Name} (Priority: {step.Priority})");
        }
    }

    /// <summary>
    /// Demonstrates custom startup actions.
    /// </summary>
    private void DemonstrateStartupActions()
    {
        Console.WriteLine("  Executing startup actions...");

        var newHost = new ApplicationHost();
        newHost.ConfigureServices(services =>
        {
            services.AddStartupAction(async provider =>
            {
                Console.WriteLine("      -> Custom startup action executed");
                await Task.Delay(10);
            }, priority: 10);
        });

        Task.Run(async () =>
        {
            await newHost.GetServices()!.InitializeAllAsync();
        }).Wait();

        Console.WriteLine($"    + Startup actions completed");
    }
}

/// <summary>
/// Database initializer with high priority.
/// </summary>
public class DatabaseInitializer : IAsyncInitializable
{
    /// <inheritdoc/>
    public int InitializationPriority => 100;

    /// <inheritdoc/>
    public IEnumerable<Type>? DependsOn => null;

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("      -> Initializing database...");
        await Task.Delay(50); // Simulate async work
        Console.WriteLine("      -> Database initialized");
    }
}

/// <summary>
/// Cache initializer with medium priority, depends on database.
/// </summary>
public class CacheInitializer : IAsyncInitializable
{
    /// <inheritdoc/>
    public int InitializationPriority => 50;

    /// <inheritdoc/>
    public IEnumerable<Type> DependsOn => new[] { typeof(DatabaseInitializer) };

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("      -> Warming up cache...");
        await Task.Delay(30); // Simulate async work
        Console.WriteLine("      -> Cache warmed up");
    }
}

/// <summary>
/// Index initializer with low priority, depends on cache.
/// </summary>
public class IndexInitializer : IAsyncInitializable
{
    /// <inheritdoc/>
    public int InitializationPriority => 10;

    /// <inheritdoc/>
    public IEnumerable<Type> DependsOn => new[] { typeof(CacheInitializer) };

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("      -> Building search indexes...");
        await Task.Delay(20); // Simulate async work
        Console.WriteLine("      -> Search indexes built");
    }
}
