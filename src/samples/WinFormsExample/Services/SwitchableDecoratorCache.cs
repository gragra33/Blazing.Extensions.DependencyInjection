using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

namespace WinFormsExample.Services;

/// <summary>
/// Cache backends available in the WinForms caching demo.
/// </summary>
public enum CacheBackend
{
    /// <summary>In-process default dictionary-backed cache.</summary>
    Default,

    /// <summary><see cref="IMemoryCache"/> adapter.</summary>
    MemoryCache,

    /// <summary><see cref="HybridCache"/> adapter.</summary>
    HybridCache,
}

/// <summary>
/// Switchable <see cref="IDecoratorCache"/> wrapper that lets the demo change cache backend at runtime.
/// </summary>
public sealed class SwitchableDecoratorCache(IServiceProvider serviceProvider) : IDecoratorCache, IDisposable
{
    private DefaultDecoratorCache? _defaultCache;
    private MemoryCacheDecoratorCache? _memoryCache;
    private HybridCacheDecoratorCache? _hybridCache;
    private volatile IDecoratorCache _current = new DefaultDecoratorCache();

    /// <summary>
    /// Gets the currently active backend.
    /// </summary>
    public CacheBackend CurrentBackend { get; private set; } = CacheBackend.Default;

    /// <summary>
    /// Switches the active cache backend.
    /// </summary>
    /// <param name="backend">Backend to activate.</param>
    public void SwitchTo(CacheBackend backend)
    {
        _current = backend switch
        {
            CacheBackend.Default => _defaultCache ??= new DefaultDecoratorCache(),
            CacheBackend.MemoryCache => _memoryCache ??= new MemoryCacheDecoratorCache(serviceProvider.GetRequiredService<IMemoryCache>()),
            CacheBackend.HybridCache => _hybridCache ??= new HybridCacheDecoratorCache(serviceProvider.GetRequiredService<HybridCache>()),
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
    public void Remove(string key)
        => _current.Remove(key);

    /// <inheritdoc/>
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => _current.RemoveByPrefixAsync(prefix, cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
        => _defaultCache?.Dispose();
}
