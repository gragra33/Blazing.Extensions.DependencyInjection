namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Pluggable caching abstraction consumed by source-generated caching decorators.
/// Register a custom implementation to swap the cache backend for all decorated services.
/// The default implementation <see cref="DefaultDecoratorCache"/> is automatically registered
/// by <c>Register()</c> via <c>TryAddSingleton</c> — your registration wins if added first.
/// </summary>
public interface IDecoratorCache
{
    /// <summary>
    /// Gets a cached value for <paramref name="key"/>, or creates and caches it using
    /// <paramref name="factory"/> with single-flight semantics.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="factory">
    /// Async factory invoked on a cache miss. Receives a <see cref="System.Threading.CancellationToken"/>
    /// from the cache implementation.
    /// </param>
    /// <param name="expiration">How long the entry should live after creation.</param>
    /// <param name="cancellationToken">Token observed during cache operations.</param>
    /// <returns>The cached or newly-created value.</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached value for <paramref name="key"/>, or creates and caches it using
    /// <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="factory">Synchronous factory invoked on a cache miss.</param>
    /// <param name="expiration">How long the entry should live after creation.</param>
    /// <returns>The cached or newly-created value.</returns>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration);

    /// <summary>Removes the cache entry identified by <paramref name="key"/> asynchronously.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Removes the cache entry identified by <paramref name="key"/> synchronously.</summary>
    /// <param name="key">The cache key to remove.</param>
    void Remove(string key);

    /// <summary>
    /// Removes all cache entries whose keys start with <paramref name="prefix"/>.
    /// Used by <see cref="IBlazingInvalidatable.InvalidateAllCacheAsync"/> and
    /// <see cref="IBlazingCacheInvalidator{TService}.InvalidateAllAsync"/>.
    /// </summary>
    /// <param name="prefix">The key prefix to match against.</param>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown by implementations that do not support prefix-based removal
    /// (e.g. <see cref="MemoryCacheDecoratorCache"/>, <see cref="HybridCacheDecoratorCache"/>,
    /// <see cref="DistributedCacheDecoratorCache"/>).
    /// Use <see cref="DefaultDecoratorCache"/> or call
    /// <see cref="RemoveAsync"/> per entry for those adapters.
    /// </exception>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Task.FromException(new NotSupportedException(
            "This IDecoratorCache implementation does not support prefix-based removal " +
            "(InvalidateAllCacheAsync / InvalidateAllAsync). " +
            "Use DefaultDecoratorCache to enable prefix-based invalidation, " +
            "or call RemoveAsync / InvalidateAsync per entry."));
}
