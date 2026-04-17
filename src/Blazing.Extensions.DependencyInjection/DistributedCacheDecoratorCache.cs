using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IDecoratorCache"/> adapter backed by <see cref="IDistributedCache"/>
/// (Redis, SQL Server, Cosmos DB, etc.).
/// Values are serialized via <see cref="IDecoratorCacheSerializer"/>; per-key
/// <see cref="SemaphoreSlim"/> locking prevents cache stampedes.
/// </summary>
/// <remarks>
/// <para>
/// Register your distributed cache and this adapter before calling <c>services.Register()</c>:
/// <code>
/// services.AddStackExchangeRedisCache(o => o.Configuration = "localhost");
/// services.AddSingleton&lt;IDecoratorCache, DistributedCacheDecoratorCache&gt;();
/// services.Register();
/// </code>
/// </para>
/// <para>
/// By default, values are serialized with <see cref="SystemTextJsonDecoratorCacheSerializer"/>.
/// Register a custom <see cref="IDecoratorCacheSerializer"/> for AOT compatibility or custom
/// serialization formats before adding this adapter.
/// </para>
/// <para>
/// <see cref="IDecoratorCache.RemoveByPrefixAsync(string, System.Threading.CancellationToken)"/> is not supported because <see cref="IDistributedCache"/>
/// does not expose key enumeration. Use <see cref="DefaultDecoratorCache"/> when prefix-based
/// invalidation is required, or call <see cref="RemoveAsync"/> per entry.
/// </para>
/// </remarks>
public sealed class DistributedCacheDecoratorCache : IDecoratorCache, IDisposable
{
    private readonly IDistributedCache _distributedCache;
    private readonly IDecoratorCacheSerializer _serializer;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="DistributedCacheDecoratorCache"/>
    /// using the provided <see cref="IDecoratorCacheSerializer"/>.
    /// </summary>
    /// <param name="distributedCache">The distributed cache backend.</param>
    /// <param name="serializer">The serializer used to convert cached values.</param>
    public DistributedCacheDecoratorCache(
        IDistributedCache distributedCache,
        IDecoratorCacheSerializer serializer)
    {
        _distributedCache = distributedCache;
        _serializer = serializer;
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var existing = await _distributedCache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return _serializer.Deserialize<T>(existing)!;

        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring the per-key lock
            existing = await _distributedCache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
                return _serializer.Deserialize<T>(existing)!;

            var result = await factory(cancellationToken).ConfigureAwait(false);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _distributedCache.SetAsync(key, _serializer.Serialize(result), options, cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
        => GetOrCreateAsync<T>(key, _ => Task.FromResult(factory()), expiration)
            .GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _distributedCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public void Remove(string key)
        => _distributedCache.RemoveAsync(key).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        foreach (var sem in _locks.Values)
            sem.Dispose();

        _locks.Clear();
    }
}
