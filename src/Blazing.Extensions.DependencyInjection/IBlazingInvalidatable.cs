namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Implemented by every source-generated caching decorator, allowing cache entries to be
/// invalidated by casting the resolved service to <see cref="IBlazingInvalidatable"/>.
/// </summary>
/// <example>
/// <code>
/// // Obtain the decorator via DI, then cast to invalidate
/// var svc = serviceProvider.GetRequiredService&lt;IProductService&gt;();
/// if (svc is IBlazingInvalidatable inv)
///     await inv.InvalidateCacheAsync("__IProductService__GetById_42");
/// </code>
/// </example>
public interface IBlazingInvalidatable
{
    /// <summary>
    /// Removes a specific cache entry by its exact computed cache key.
    /// </summary>
    /// <param name="cacheKey">
    /// The full cache key, e.g. <c>__IProductService__GetById_42</c>.
    /// </param>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    Task InvalidateCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries belonging to this decorator instance (prefix-based removal).
    /// </summary>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when the registered <see cref="IDecoratorCache"/> implementation does not support
    /// prefix-based removal. Use <see cref="DefaultDecoratorCache"/> to enable this operation.
    /// </exception>
    Task InvalidateAllCacheAsync(
        CancellationToken cancellationToken = default);
}
