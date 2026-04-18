using Microsoft.Extensions.Caching.Hybrid;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IDecoratorCache"/> adapter backed by <see cref="HybridCache"/>
/// (L1 in-memory + optional L2 distributed cache with stampede protection).
/// Requires .NET 9+ and the <c>Microsoft.Extensions.Caching.Hybrid</c> package.
/// </summary>
/// <remarks>
/// <para>
/// Register <see cref="HybridCache"/> via <c>services.AddHybridCache()</c> before calling
/// <c>services.Register()</c>, then register this adapter:
/// <code>
/// services.AddHybridCache();
/// services.AddSingleton&lt;IDecoratorCache, HybridCacheDecoratorCache&gt;();
/// services.Register();
/// </code>
/// </para>
/// <para>
/// <strong>Sync path warning:</strong> <see cref="HybridCache"/> is async-only.
/// <see cref="GetOrCreate{T}"/> blocks on the async path via
/// <c>GetAwaiter().GetResult()</c>. Avoid calling synchronous interface methods under
/// <c>[CachingDecorator]</c> from a thread with a <c>SynchronizationContext</c> (e.g. ASP.NET
/// classic, WinForms UI thread) to prevent deadlocks. All async methods are preferred.
/// </para>
/// <para>
/// <see cref="IDecoratorCache.RemoveByPrefixAsync(string, System.Threading.CancellationToken)"/> is not supported. Use <see cref="DefaultDecoratorCache"/>
/// when prefix-based invalidation is required.
/// </para>
/// </remarks>
public sealed class HybridCacheDecoratorCache(HybridCache hybridCache) : IDecoratorCache
{
    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var options = new HybridCacheEntryOptions { Expiration = expiration };
        // HybridCache expects Func<CancellationToken, ValueTask<T>>; wrap the Task<T> factory.
        Func<CancellationToken, ValueTask<T>> hybridFactory = async ct =>
            await factory(ct).ConfigureAwait(false);

        return await hybridCache.GetOrCreateAsync(
            key,
            hybridFactory,
            options,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
    {
        // HybridCache is async-only; block synchronously. See remarks for deadlock guidance.
        // Wrap the synchronous factory into a ValueTask<T> delegate for the HybridCache API.
        var options = new HybridCacheEntryOptions { Expiration = expiration };
        Func<CancellationToken, ValueTask<T>> hybridFactory = _ => ValueTask.FromResult(factory());
        return hybridCache.GetOrCreateAsync(
            key,
            hybridFactory,
            options).AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await hybridCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public void Remove(string key)
        => hybridCache.RemoveAsync(key).AsTask().GetAwaiter().GetResult();
}
