namespace BlazorServerExample.Services;

/// <summary>
/// Concrete product catalog service that simulates a slow data store.
/// The <see cref="CachingDecoratorAttribute"/> causes the source generator to wrap
/// every <see cref="IProductCatalogService"/> method result in <see cref="IDecoratorCache"/>,
/// so the backend is only called on a cache miss.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
[CachingDecorator(seconds: 30)]
public class ProductCatalogService : IProductCatalogService
{
    private static readonly Dictionary<int, string> _catalog = new()
    {
        [1] = "Laptop Pro",
        [2] = "Wireless Mouse",
        [3] = "USB-C Hub",
        [4] = "Mechanical Keyboard",
        [5] = "27\" Monitor",
    };

    private static readonly Dictionary<int, int> _stock = new()
    {
        [1] = 12, [2] = 45, [3] = 78, [4] = 30, [5] = 7,
    };

    private static int _callCount;

    /// <summary>
    /// Gets the number of times the real backend has been invoked.
    /// This is used by the sample UI to detect cache hits vs misses.
    /// </summary>
    public static int TotalBackendCallCount => _callCount;

    /// <inheritdoc/>
    public string GetName(int id)
    {
        Interlocked.Increment(ref _callCount);
        Thread.Sleep(60); // simulate synchronous DB round-trip
        return _catalog.TryGetValue(id, out var name) ? name : $"Unknown Product #{id}";
    }

    /// <inheritdoc/>
    public async Task<string> GetNameAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(60); // simulate async DB round-trip
        return _catalog.TryGetValue(id, out var name) ? name : $"Unknown Product #{id}";
    }

    /// <inheritdoc/>
    public async ValueTask<int> GetCountAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(60); // simulate async DB round-trip
        return _stock.GetValueOrDefault(id, 0);
    }
}
