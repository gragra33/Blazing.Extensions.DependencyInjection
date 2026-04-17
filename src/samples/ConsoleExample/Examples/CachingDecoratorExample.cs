using Microsoft.Extensions.Caching.Hybrid;

namespace ConsoleExample.Examples;

// ── Service interface + implementation for this demo ──────────────────────────

/// <summary>Product catalog service interface used by the caching decorator demo.</summary>
public interface ICatalogService
{
    /// <summary>Returns the product name for <paramref name="id"/> (sync, cached).</summary>
    string GetName(int id);

    /// <summary>Returns the product name for <paramref name="id"/> (Task&lt;T&gt;, cached).</summary>
    Task<string> GetNameAsync(int id);

    /// <summary>Returns the stock count for <paramref name="id"/> (ValueTask&lt;T&gt;, cached).</summary>
    ValueTask<int> GetCountAsync(int id);
}

/// <summary>
/// Concrete catalog service that simulates a slow data store.
/// The source generator wraps every <see cref="ICatalogService"/> method result via
/// <see cref="IDecoratorCache"/> because of <c>[CachingDecorator(seconds: 10)]</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
[CachingDecorator(seconds: 10)]
public class CatalogService : ICatalogService
{
    private static readonly Dictionary<int, string> _names = new()
    {
        [1] = "Laptop Pro", [2] = "Wireless Mouse", [3] = "USB-C Hub",
    };

    private static readonly Dictionary<int, int> _stock = new()
    {
        [1] = 12, [2] = 45, [3] = 78,
    };

    private static int _callCount;

    /// <summary>
    /// Gets the number of times the real backend has been invoked.
    /// This is used by the sample output to detect cache hits vs misses.
    /// </summary>
    public static int TotalBackendCallCount => _callCount;

    /// <inheritdoc/>
    public string GetName(int id)
    {
        Interlocked.Increment(ref _callCount);
        Thread.Sleep(40); // simulate 40 ms DB round-trip
        return _names.TryGetValue(id, out var n) ? n : $"Unknown #{id}";
    }

    /// <inheritdoc/>
    public async Task<string> GetNameAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(40);
        return _names.TryGetValue(id, out var n) ? n : $"Unknown #{id}";
    }

    /// <inheritdoc/>
    public async ValueTask<int> GetCountAsync(int id)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Delay(40);
        return _stock.TryGetValue(id, out var c) ? c : 0;
    }
}

/// <summary>The available backends for <see cref="DemoSwitchableDecoratorCache"/>.</summary>
public enum DemoCacheBackend
{
    /// <summary>Uses <see cref="DefaultDecoratorCache"/>.</summary>
    Default,

    /// <summary>Uses <see cref="MemoryCacheDecoratorCache"/>.</summary>
    MemoryCache,

    /// <summary>Uses <see cref="HybridCacheDecoratorCache"/>.</summary>
    HybridCache,
}

/// <summary>
/// Sample-level <see cref="IDecoratorCache"/> wrapper allowing runtime backend switching
/// without restarting the host.
/// </summary>
public sealed class DemoSwitchableDecoratorCache(IServiceProvider serviceProvider) : IDecoratorCache, IDisposable
{
    private DefaultDecoratorCache? _defaultCache;
    private MemoryCacheDecoratorCache? _memoryCache;
    private HybridCacheDecoratorCache? _hybridCache;

    private volatile IDecoratorCache _current = new DefaultDecoratorCache();

    /// <summary>Gets the currently active backend.</summary>
    public DemoCacheBackend CurrentBackend { get; private set; } = DemoCacheBackend.Default;

    /// <summary>Switches to the selected backend.</summary>
    /// <param name="backend">Backend to activate.</param>
    public void SwitchTo(DemoCacheBackend backend)
    {
        _current = backend switch
        {
            DemoCacheBackend.Default => _defaultCache ??= new DefaultDecoratorCache(),
            DemoCacheBackend.MemoryCache => _memoryCache ??= new MemoryCacheDecoratorCache(serviceProvider.GetRequiredService<IMemoryCache>()),
            DemoCacheBackend.HybridCache => _hybridCache ??= new HybridCacheDecoratorCache(serviceProvider.GetRequiredService<HybridCache>()),
            _ => throw new ArgumentOutOfRangeException(nameof(backend)),
        };

        CurrentBackend = backend;
    }

    /// <inheritdoc/>
    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
        => _current.GetOrCreateAsync(key, factory, expiration, cancellationToken);

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
        => _current.GetOrCreate(key, factory, expiration);

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _current.RemoveAsync(key, cancellationToken);

    /// <inheritdoc/>
    public void Remove(string key) => _current.Remove(key);

    /// <inheritdoc/>
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => _current.RemoveByPrefixAsync(prefix, cancellationToken);

    /// <inheritdoc/>
    public void Dispose() => _defaultCache?.Dispose();
}

// ── Example ───────────────────────────────────────────────────────────────────

/// <summary>
/// Demonstrates the <c>[CachingDecorator]</c> source-generated caching pattern with all three
/// return-type variants (sync, <see cref="Task{T}"/>, <see cref="ValueTask{T}"/>), per-key
/// invalidation via <see cref="IBlazingCacheInvalidator{T}"/>, and runtime backend switching
/// through <see cref="DemoSwitchableDecoratorCache"/>.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class CachingDecoratorExample : IExample
{
    /// <inheritdoc/>
    public string Name => "Caching Decorator Demo";

    /// <inheritdoc/>
    public void Run() => RunAsync().GetAwaiter().GetResult();

    private static async Task RunAsync()
    {
        var host = new ApplicationHost();
        host.AddAssembly(typeof(CachingDecoratorExample).Assembly)
            .ConfigureServices(services =>
            {
                services.AddMemoryCache();
                services.AddHybridCache();
                services.AddSingleton<DemoSwitchableDecoratorCache>();
                services.AddSingleton<IDecoratorCache>(sp => sp.GetRequiredService<DemoSwitchableDecoratorCache>());
                services.Register();
            });

        var catalog = host.GetRequiredService<ICatalogService>();
        var invalidator = host.GetRequiredService<IBlazingCacheInvalidator<ICatalogService>>();
        var switchable = host.GetRequiredService<DemoSwitchableDecoratorCache>();

        Section("1 — Default backend (ConcurrentDictionary)");
        await RunMissThenHitSequence(catalog);

        Section("2 — Switch to MemoryCache backend");
        switchable.SwitchTo(DemoCacheBackend.MemoryCache);
        Console.WriteLine("    Switched backend to MemoryCache.");
        await RunMissThenHitSequence(catalog);

        Section("3 — Switch to HybridCache backend");
        switchable.SwitchTo(DemoCacheBackend.HybridCache);
        Console.WriteLine("    Switched backend to HybridCache.");
        await RunMissThenHitSequence(catalog);

        Section("4 — Per-key invalidation on current backend");
        await invalidator.InvalidateAsync(nameof(ICatalogService.GetName), ["1"]);
        await invalidator.InvalidateAsync(nameof(ICatalogService.GetNameAsync), ["2"]);
        await invalidator.InvalidateAsync(nameof(ICatalogService.GetCountAsync), ["3"]);
        Console.WriteLine("    Invalidated GetName(#1), GetNameAsync(#2), GetCountAsync(#3)");

        var before = CatalogService.TotalBackendCallCount;
        await CallSync(catalog, 1);
        await CallTaskT(catalog, 2);
        await CallValueTaskT(catalog, 3);
        Console.WriteLine($"    Backend calls after invalidation: {CatalogService.TotalBackendCallCount - before} (expected 3)");

        Console.WriteLine();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task RunMissThenHitSequence(ICatalogService catalog)
    {
        SubSection("First calls (expected misses)");
        var before = CatalogService.TotalBackendCallCount;
        await CallSync(catalog, 1);
        await CallTaskT(catalog, 2);
        await CallValueTaskT(catalog, 3);
        Console.WriteLine($"    Backend calls: {CatalogService.TotalBackendCallCount - before} (expected 3)");

        SubSection("Second calls (expected hits)");
        before = CatalogService.TotalBackendCallCount;
        await CallSync(catalog, 1);
        await CallTaskT(catalog, 2);
        await CallValueTaskT(catalog, 3);
        Console.WriteLine($"    Backend calls: {CatalogService.TotalBackendCallCount - before} (expected 0)");
    }

    private static async Task CallSync(ICatalogService catalog, int id)
    {
        var before = CatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await Task.Run(() => catalog.GetName(id));
        sw.Stop();
        var hit = CatalogService.TotalBackendCallCount == before;
        Console.WriteLine($"    GetName(#{id}) → \"{result}\" [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]");
    }

    private static async Task CallTaskT(ICatalogService catalog, int id)
    {
        var before = CatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await catalog.GetNameAsync(id);
        sw.Stop();
        var hit = CatalogService.TotalBackendCallCount == before;
        Console.WriteLine($"    GetNameAsync(#{id}) → \"{result}\" [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]");
    }

    private static async Task CallValueTaskT(ICatalogService catalog, int id)
    {
        var before = CatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await catalog.GetCountAsync(id);
        sw.Stop();
        var hit = CatalogService.TotalBackendCallCount == before;
        Console.WriteLine($"    GetCountAsync(#{id}) → stock: {result} [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]");
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"  ═══ {title} ═══");
    }

    private static void SubSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"  ── {title}");
    }
}
