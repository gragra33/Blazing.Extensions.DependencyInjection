namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Injectable per-service cache invalidator emitted alongside each source-generated caching decorator.
/// Registered automatically by <c>Register()</c> as
/// <c>IBlazingCacheInvalidator&lt;<typeparamref name="TService"/>&gt;</c>.
/// </summary>
/// <typeparam name="TService">The decorated service interface (e.g. <c>IProductService</c>).</typeparam>
/// <example>
/// <code>
/// // Inject and call from any service/controller
/// public class ProductController(IBlazingCacheInvalidator&lt;IProductService&gt; invalidator)
/// {
///     public async Task RefreshAsync(int id, CancellationToken ct)
///         => await invalidator.InvalidateAsync("GetById", id.ToString());
/// }
/// </code>
/// </example>
public interface IBlazingCacheInvalidator<TService>
{
    /// <summary>Gets the service type for which this invalidator manages cache entries.</summary>
    Type TargetServiceType => typeof(TService);

    /// <summary>
    /// Removes the cache entry for a specific method call.
    /// The cache key is derived from <paramref name="methodName"/> and
    /// <paramref name="argValues"/> using the same format the generated decorator uses.
    /// </summary>
    /// <param name="methodName">The method name as it appears on the interface (e.g. <c>"GetById"</c>).</param>
    /// <param name="argValues">
    /// The string representations of each argument, in parameter order
    /// (e.g. <c>"42"</c> for an <c>int id = 42</c> parameter).
    /// Pass an empty span to invalidate a zero-argument method.
    /// </param>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    Task InvalidateAsync(
        string methodName,
        ReadOnlySpan<string> argValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the cache entry identified by the exact pre-computed <paramref name="cacheKey"/>.
    /// </summary>
    /// <param name="cacheKey">
    /// The full cache key, e.g. <c>__IProductService__GetById_42</c>.
    /// </param>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    Task InvalidateAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries for <typeparamref name="TService"/> (prefix-based removal).
    /// </summary>
    /// <param name="cancellationToken">Token observed during the remove operation.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when the registered <see cref="IDecoratorCache"/> implementation does not support
    /// prefix-based removal. Use <see cref="DefaultDecoratorCache"/> to enable this operation.
    /// </exception>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}
