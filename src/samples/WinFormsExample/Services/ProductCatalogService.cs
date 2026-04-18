namespace WinFormsExample.Services;

/// <summary>
/// Slow backend implementation used by the caching demo.
/// Decorated via <see cref="CachingDecoratorAttribute"/> so method results are cached.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
[CachingDecorator(seconds: 30)]
public class ProductCatalogService : IProductCatalogService
{
    private static readonly Dictionary<int, string> Catalog = new()
    {
        [1] = "Laptop Pro",
        [2] = "Wireless Mouse",
        [3] = "USB-C Hub",
        [4] = "Mechanical Keyboard",
        [5] = "27\" Monitor",
    };

    private static readonly Dictionary<int, int> Stock = new()
    {
        [1] = 12, [2] = 45, [3] = 78, [4] = 30, [5] = 7,
    };

    private static int _callCount;

    /// <summary>
    /// Gets the total number of backend calls made by this service.
    /// </summary>
    public static int TotalBackendCallCount => _callCount;

    /// <inheritdoc/>
    public string GetName(int id)
    {
        Interlocked.Increment(ref _callCount);
        Thread.Sleep(60);
        return Catalog.TryGetValue(id, out var name) ? name : $"Unknown Product #{id}";
    }

    /// <inheritdoc/>
    public async Task<string> GetNameAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(60);
        return Catalog.TryGetValue(id, out var name) ? name : $"Unknown Product #{id}";
    }

    /// <inheritdoc/>
    public async ValueTask<int> GetCountAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(60);
        return Stock.TryGetValue(id, out var count) ? count : 0;
    }
}
