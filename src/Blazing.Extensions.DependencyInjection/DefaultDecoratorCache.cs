using System.Collections.Concurrent;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Default <see cref="IDecoratorCache"/> implementation backed by an in-process
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> with per-key <see cref="SemaphoreSlim"/>
/// locking for stampede protection.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is automatically registered by <c>Register()</c> via
/// <c>TryAddSingleton</c> — register your own <see cref="IDecoratorCache"/> before calling
/// <c>Register()</c> to replace it.
/// </para>
/// <para>
/// Unlike <see cref="MemoryCacheDecoratorCache"/> and other adapters,
/// <see cref="DefaultDecoratorCache"/> supports prefix-based removal via
/// <see cref="IDecoratorCache.RemoveByPrefixAsync(string, System.Threading.CancellationToken)"/> because it maintains its own key list.
/// </para>
/// </remarks>
public sealed class DefaultDecoratorCache : IDecoratorCache, IDisposable
{
    private sealed record CacheEntry(object? Value, DateTime Expiry);

    private readonly ConcurrentDictionary<string, CacheEntry> _store = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);
    private bool _disposed;

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Fast path — entry exists and has not expired
        if (_store.TryGetValue(key, out var existing) && DateTime.UtcNow < existing.Expiry)
            return (T)existing.Value!;

        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check after acquiring the per-key lock (double-checked locking)
            if (_store.TryGetValue(key, out existing) && DateTime.UtcNow < existing.Expiry)
                return (T)existing.Value!;

            var result = await factory(cancellationToken).ConfigureAwait(false);
            _store[key] = new CacheEntry(result, DateTime.UtcNow.Add(expiration));
            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Fast path
        if (_store.TryGetValue(key, out var existing) && DateTime.UtcNow < existing.Expiry)
            return (T)existing.Value!;

        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        semaphore.Wait();
        try
        {
            if (_store.TryGetValue(key, out existing) && DateTime.UtcNow < existing.Expiry)
                return (T)existing.Value!;

            var result = factory();
            _store[key] = new CacheEntry(result, DateTime.UtcNow.Add(expiration));
            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        _store.TryRemove(key, out _);
        if (_locks.TryRemove(key, out var sem))
            sem.Dispose();
    }

    /// <inheritdoc/>
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Materialize to a list first to avoid modifying the dictionary while iterating.
        var keys = _store.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keys)
        {
            _store.TryRemove(key, out _);
            if (_locks.TryRemove(key, out var sem))
                sem.Dispose();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _store.Clear();
        foreach (var sem in _locks.Values)
            sem.Dispose();

        _locks.Clear();
    }
}
