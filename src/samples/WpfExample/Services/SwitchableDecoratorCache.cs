namespace WpfExample.Services;

/// <summary>The three available cache backends for the runtime-switching demo.</summary>
public enum CacheBackend { Default, MemoryCache, HybridCache }

/// <summary>
/// Singleton <see cref="IDecoratorCache"/> wrapper that lazily initialises each backend
/// on first use and allows the active backend to be swapped at runtime without restarting
/// the application.
/// </summary>
/// <remarks>
/// Register this before <c>services.Register()</c> so the source-generated
/// <c>TryAddSingleton&lt;IDecoratorCache, DefaultDecoratorCache&gt;()</c> does not override it:
/// <code>
/// services.AddMemoryCache();
/// services.AddHybridCache();
/// services.AddSingleton&lt;SwitchableDecoratorCache&gt;();
/// services.AddSingleton&lt;IDecoratorCache&gt;(sp =&gt; sp.GetRequiredService&lt;SwitchableDecoratorCache&gt;());
/// services.Register();
/// </code>
/// </remarks>
public sealed class SwitchableDecoratorCache(IServiceProvider sp) : IDecoratorCache, IDisposable
{
    // Lazily-created backends — only instantiated when first switched to.
    private DefaultDecoratorCache? _defaultCache;
    private MemoryCacheDecoratorCache? _memoryCacheAdapter;
    private HybridCacheDecoratorCache? _hybridCacheAdapter;

    // Volatile for lock-free reads; write is always from the UI thread in this sample.
    private volatile IDecoratorCache _current = new DefaultDecoratorCache();

    /// <summary>Gets the currently active cache backend.</summary>
    public CacheBackend CurrentBackend { get; private set; } = CacheBackend.Default;

    /// <summary>
    /// Switches the active cache backend.  The new backend is lazily created on first switch.
    /// Entries cached under the previous backend remain in that backend's store and will expire
    /// naturally; they are not migrated.
    /// </summary>
    public void SwitchTo(CacheBackend backend)
    {
        _current = backend switch
        {
            CacheBackend.Default =>
                _defaultCache ??= new DefaultDecoratorCache(),
            CacheBackend.MemoryCache =>
                _memoryCacheAdapter ??= new MemoryCacheDecoratorCache(
                    sp.GetRequiredService<IMemoryCache>()),
            CacheBackend.HybridCache =>
                _hybridCacheAdapter ??= new HybridCacheDecoratorCache(
                    sp.GetRequiredService<HybridCache>()),
            _ => throw new ArgumentOutOfRangeException(nameof(backend))
        };
        CurrentBackend = backend;
    }

    // ── IDecoratorCache delegation ────────────────────────────────────────────

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
    /// <remarks>Only supported when the active backend is <see cref="DefaultDecoratorCache"/>.</remarks>
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => _current.RemoveByPrefixAsync(prefix, cancellationToken);

    /// <inheritdoc/>
    public void Dispose() => _defaultCache?.Dispose();
}
