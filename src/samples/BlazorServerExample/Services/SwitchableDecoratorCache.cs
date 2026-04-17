namespace BlazorServerExample.Services;

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
    private CacheAccessInfo? _lastAccess;
    private int _totalCacheHits;
    private int _totalCacheMisses;

    /// <summary>Gets the currently active cache backend.</summary>
    public CacheBackend CurrentBackend { get; private set; } = CacheBackend.Default;

    /// <summary>Gets the latest cache access information (hit/miss, backend, and key).</summary>
    public CacheAccessInfo? LastAccess => _lastAccess;

    /// <summary>Gets the total number of cache hits observed by this switchable wrapper.</summary>
    public int TotalCacheHits => _totalCacheHits;

    /// <summary>Gets the total number of cache misses observed by this switchable wrapper.</summary>
    public int TotalCacheMisses => _totalCacheMisses;

    /// <summary>
    /// Gets the number of backend calls inferred from cache misses.
    /// Equivalent to <see cref="TotalCacheMisses"/>.
    /// </summary>
    public int TotalBackendCalls => _totalCacheMisses;

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
        => GetOrCreateWithTelemetryAsync(key, factory, expiration, cancellationToken);

    private async Task<T> GetOrCreateWithTelemetryAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken)
    {
        var backend = CurrentBackend;
        var factoryCalled = false;

        var result = await _current.GetOrCreateAsync(
            key,
            async ct =>
            {
                factoryCalled = true;
                return await factory(ct).ConfigureAwait(false);
            },
            expiration,
            cancellationToken).ConfigureAwait(false);

        var hit = !factoryCalled;
        RecordAccess(key, backend, hit);
        return result;
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
    {
        var backend = CurrentBackend;
        var factoryCalled = false;

        var result = _current.GetOrCreate(
            key,
            () =>
            {
                factoryCalled = true;
                return factory();
            },
            expiration);

        var hit = !factoryCalled;
        RecordAccess(key, backend, hit);
        return result;
    }

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

    private void RecordAccess(string key, CacheBackend backend, bool hit)
    {
        if (hit)
            Interlocked.Increment(ref _totalCacheHits);
        else
            Interlocked.Increment(ref _totalCacheMisses);

        _lastAccess = new CacheAccessInfo(key, backend, hit, DateTime.Now);
    }
}

/// <summary>
/// Describes one cache access result captured by <see cref="SwitchableDecoratorCache"/>.
/// </summary>
/// <param name="Key">The generated cache key used for the operation.</param>
/// <param name="Backend">The active backend at the time of access.</param>
/// <param name="Hit"><see langword="true"/> when value came from cache; otherwise factory executed (backend miss).</param>
/// <param name="Timestamp">Local timestamp of the access record.</param>
public sealed record CacheAccessInfo(string Key, CacheBackend Backend, bool Hit, DateTime Timestamp);
