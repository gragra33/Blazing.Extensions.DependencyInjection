using Microsoft.Extensions.Caching.Memory;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IDecoratorCache"/> adapter backed by <see cref="IMemoryCache"/>.
/// Suitable for single-server, in-process scenarios where the host's own memory cache is preferred.
/// </summary>
/// <remarks>
/// <para>
/// Register <see cref="IMemoryCache"/> via <c>services.AddMemoryCache()</c> before calling
/// <c>services.Register()</c>, then register this adapter:
/// <code>
/// services.AddMemoryCache();
/// services.AddSingleton&lt;IDecoratorCache, MemoryCacheDecoratorCache&gt;();
/// services.Register();
/// </code>
/// </para>
/// <para>
/// <see cref="IDecoratorCache.RemoveByPrefixAsync(string, System.Threading.CancellationToken)"/> is not supported because <see cref="IMemoryCache"/> does not
/// expose an enumerable key set. Use <see cref="DefaultDecoratorCache"/> when prefix-based
/// invalidation (<see cref="IBlazingInvalidatable.InvalidateAllCacheAsync"/> /
/// <see cref="IBlazingCacheInvalidator{TService}.InvalidateAllAsync"/>) is required.
/// </para>
/// </remarks>
public sealed class MemoryCacheDecoratorCache(IMemoryCache memoryCache) : IDecoratorCache
{
    /// <inheritdoc/>
    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
        => memoryCache.GetOrCreateAsync<T>(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration;
            return factory(cancellationToken);
        })!;

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
        => memoryCache.GetOrCreate<T>(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration;
            return factory();
        })!;

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Remove(string key) => memoryCache.Remove(key);
}
