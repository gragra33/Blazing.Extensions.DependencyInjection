namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates service decoration patterns for cross-cutting concerns like caching, logging, and validation.
/// Shows decorator pattern implementation for wrapping services with additional behavior.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class ServiceDecorationExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Service Decoration Patterns";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDecorationExample"/> class.
    /// </summary>
    public ServiceDecorationExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateCachedRepository();
        DemonstrateLoggingRepository();
        DemonstrateMultipleDecorators();
    }

    /// <summary>
    /// Configures decorated services.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            // Caching decorator
            services.AddSingleton<IDataRepository>(_ =>
            {
                var inner = new DataRepository();
                return new CachedDataRepository(inner);
            });

            // Logging decorator
            services.AddSingleton<IOrderService>(_ =>
            {
                var inner = new OrderService();
                return new LoggingOrderService(inner);
            });

            // Multiple decorators (logging + caching)
            services.AddSingleton<IProductService>(_ =>
            {
                var baseService = new ProductService();
                var logged = new LoggingProductService(baseService);
                return new CachedProductService(logged);
            });
        });
    }

    /// <summary>
    /// Demonstrates caching decorator pattern.
    /// </summary>
    private void DemonstrateCachedRepository()
    {
        Console.WriteLine("  Testing cached repository...");

        var repo = _host.GetRequiredService<IDataRepository>();

        var data1 = repo.GetData("key1");
        var data2 = repo.GetData("key1"); // Should hit cache

        Console.WriteLine($"    + First call: {data1}");
        Console.WriteLine($"    + Second call (cached): {data2}");
        Console.WriteLine("    + Caching decorator applied successfully");
    }

    /// <summary>
    /// Demonstrates logging decorator pattern.
    /// </summary>
    private void DemonstrateLoggingRepository()
    {
        Console.WriteLine("  Testing logging decorator...");

        var service = _host.GetRequiredService<IOrderService>();
        service.ProcessOrder("Order123");

        Console.WriteLine("    + Logging decorator captured method call");
    }

    /// <summary>
    /// Demonstrates multiple decorator chains (logging + caching).
    /// </summary>
    private void DemonstrateMultipleDecorators()
    {
        Console.WriteLine("  Testing multiple decorators...");

        var service = _host.GetRequiredService<IProductService>();

        service.GetProduct("P001");
        service.GetProduct("P001"); // Cached

        Console.WriteLine("    + Multiple decorators applied (logging -> caching)");
    }
}

/// <summary>Data repository interface.</summary>
public interface IDataRepository
{
    /// <summary>Gets data by key.</summary>
    string GetData(string key);
}

/// <summary>Base data repository.</summary>
public class DataRepository : IDataRepository
{
    /// <inheritdoc/>
    public string GetData(string key) => $"Data for {key}";
}

/// <summary>Cached data repository decorator.</summary>
public class CachedDataRepository : IDataRepository
{
    private readonly IDataRepository _inner;
    private readonly Dictionary<string, string> _cache = new();

    /// <summary>Initializes decorator with inner repository.</summary>
    public CachedDataRepository(IDataRepository inner) => _inner = inner;

    /// <inheritdoc/>
    public string GetData(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var data = _inner.GetData(key);
        _cache[key] = data;
        return data;
    }
}

/// <summary>Order service interface.</summary>
public interface IOrderService
{
    /// <summary>Processes an order.</summary>
    void ProcessOrder(string orderId);
}

/// <summary>Base order service.</summary>
public class OrderService : IOrderService
{
    /// <inheritdoc/>
    public void ProcessOrder(string orderId) { }
}

/// <summary>
/// Logging order service decorator.
/// </summary>
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _inner;

    /// <summary>Initializes decorator with inner service.</summary>
    public LoggingOrderService(IOrderService inner) => _inner = inner;

    /// <inheritdoc/>
    public void ProcessOrder(string orderId)
    {
        Console.WriteLine($"      -> Logging: Processing order {orderId}");
        _inner.ProcessOrder(orderId);
        Console.WriteLine($"      -> Logging: Completed order {orderId}");
    }
}

/// <summary>Product service interface.</summary>
public interface IProductService
{
    /// <summary>Gets a product.</summary>
    string GetProduct(string id);
}

/// <summary>Base product service.</summary>
public class ProductService : IProductService
{
    /// <inheritdoc/>
    public string GetProduct(string id) => $"Product {id}";
}

/// <summary>Logging product service decorator.</summary>
public class LoggingProductService : IProductService
{
    private readonly IProductService _inner;

    /// <summary>Initializes decorator with inner service.</summary>
    public LoggingProductService(IProductService inner) => _inner = inner;

    /// <inheritdoc/>
    public string GetProduct(string id)
    {
        Console.WriteLine($"      -> Log: Getting product {id}");
        return _inner.GetProduct(id);
    }
}

/// <summary>Cached product service decorator.</summary>
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly Dictionary<string, string> _cache = new();

    /// <summary>Initializes decorator with inner service.</summary>
    public CachedProductService(IProductService inner) => _inner = inner;

    /// <inheritdoc/>
    public string GetProduct(string id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"      -> Cache: Hit for {id}");
            return cached;
        }

        Console.WriteLine($"      -> Cache: Miss for {id}");
        var product = _inner.GetProduct(id);
        _cache[id] = product;
        return product;
    }
}
