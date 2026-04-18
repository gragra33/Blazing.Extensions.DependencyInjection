using Microsoft.Extensions.Caching.Memory;
using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="MemoryCacheDecoratorCache"/> verifying that it correctly
/// delegates to an underlying <see cref="IMemoryCache"/> instance.
/// </summary>
public sealed class MemoryCacheDecoratorCacheTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly IDecoratorCache _cache;

    /// <summary>Initializes the test fixture.</summary>
    public MemoryCacheDecoratorCacheTests()
        => _cache = new MemoryCacheDecoratorCache(_memoryCache);

    /// <inheritdoc/>
    public void Dispose() => _memoryCache.Dispose();

    /// <summary>
    /// Verifies that <c>GetOrCreateAsync</c> stores and retrieves a value.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_NewKey_ReturnsFactoryValue()
    {
        var result = await _cache.GetOrCreateAsync<string>(
            "k1",
            async _ => await Task.FromResult("from-factory"),
            TimeSpan.FromMinutes(5));

        result.ShouldBe("from-factory");
    }

    /// <summary>
    /// Verifies that a second call with the same key does not invoke the factory again.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ExistingKey_ReturnsCachedValue()
    {
        var callCount = 0;
        Func<CancellationToken, Task<string>> factory = async _ =>
        {
            callCount++;
            return await Task.FromResult("cached");
        };

        await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));
        var second = await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));

        second.ShouldBe("cached");
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that <c>GetOrCreate</c> (synchronous) stores and retrieves a value.
    /// </summary>
    [Fact]
    public void GetOrCreate_NewKey_ReturnsFactoryValue()
    {
        var result = _cache.GetOrCreate("k3", () => 99, TimeSpan.FromMinutes(5));

        result.ShouldBe(99);
    }

    /// <summary>
    /// Verifies that <c>RemoveAsync</c> evicts the entry from the underlying <see cref="IMemoryCache"/>.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ExistingKey_EvictsFromMemoryCache()
    {
        var callCount = 0;
        Func<CancellationToken, Task<int>> factory = async _ =>
        {
            callCount++;
            return await Task.FromResult(callCount);
        };

        await _cache.GetOrCreateAsync("k4", factory, TimeSpan.FromMinutes(5));
        await _cache.RemoveAsync("k4");
        await _cache.GetOrCreateAsync("k4", factory, TimeSpan.FromMinutes(5));

        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>Remove</c> (synchronous) evicts the entry.
    /// </summary>
    [Fact]
    public void Remove_ExistingKey_EvictsFromMemoryCache()
    {
        var callCount = 0;
        _cache.GetOrCreate("k5", () => { callCount++; return "v"; }, TimeSpan.FromMinutes(5));
        _cache.Remove("k5");
        _cache.GetOrCreate("k5", () => { callCount++; return "v"; }, TimeSpan.FromMinutes(5));

        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>RemoveByPrefixAsync</c> throws <see cref="NotSupportedException"/>
    /// because <see cref="IMemoryCache"/> does not support prefix-based scanning.
    /// </summary>
    [Fact]
    public async Task RemoveByPrefixAsync_ThrowsNotSupportedException()
    {
        await Should.ThrowAsync<NotSupportedException>(
            () => _cache.RemoveByPrefixAsync("__IAnything__"));
    }
}
