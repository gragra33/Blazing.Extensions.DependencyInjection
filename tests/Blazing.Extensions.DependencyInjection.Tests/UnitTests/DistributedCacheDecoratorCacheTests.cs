using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="DistributedCacheDecoratorCache"/> verifying that it correctly
/// delegates to an underlying <see cref="IDistributedCache"/> instance using a
/// <see cref="MemoryDistributedCache"/> as a real in-process backend.
/// </summary>
public sealed class DistributedCacheDecoratorCacheTests : IDisposable
{
    private readonly MemoryDistributedCache _distributedCache;
    private readonly IDecoratorCache _cache;

    /// <summary>Initializes the test fixture with a real in-memory distributed cache.</summary>
    public DistributedCacheDecoratorCacheTests()
    {
        _distributedCache = new MemoryDistributedCache(
            new Microsoft.Extensions.Options.OptionsWrapper<MemoryDistributedCacheOptions>(
                new MemoryDistributedCacheOptions()));

        _cache = new DistributedCacheDecoratorCache(
            _distributedCache,
            new SystemTextJsonDecoratorCacheSerializer());
    }

    /// <inheritdoc/>
    public void Dispose() => (_cache as IDisposable)?.Dispose();

    /// <summary>
    /// Verifies that <c>GetOrCreateAsync</c> calls the factory for a new key and stores
    /// the serialized value in the distributed cache.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_NewKey_CallsFactoryAndReturnsValue()
    {
        var callCount = 0;
        var result = await _cache.GetOrCreateAsync<string>(
            "k1",
            async _ => { callCount++; return await Task.FromResult("distributed-value"); },
            TimeSpan.FromMinutes(5));

        result.ShouldBe("distributed-value");
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that a second call for the same key returns the cached (deserialized) value
    /// without invoking the factory again.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ExistingKey_ReturnsCachedValue()
    {
        var callCount = 0;
        Func<CancellationToken, Task<int>> factory = async _ =>
        {
            callCount++;
            return await Task.FromResult(42);
        };

        await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));
        var second = await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));

        second.ShouldBe(42);
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that <c>GetOrCreate</c> (synchronous) calls the factory and caches the result.
    /// </summary>
    [Fact]
    public void GetOrCreate_NewKey_CallsFactoryAndReturnsValue()
    {
        var callCount = 0;
        var result = _cache.GetOrCreate("k3", () => { callCount++; return "sync-value"; }, TimeSpan.FromMinutes(5));

        result.ShouldBe("sync-value");
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that <c>RemoveAsync</c> evicts the entry so the factory is called again.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ExistingKey_EvictsEntry()
    {
        var callCount = 0;
        Func<CancellationToken, Task<string>> factory = async _ =>
        {
            callCount++;
            return await Task.FromResult("value");
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
    public void Remove_ExistingKey_EvictsEntry()
    {
        var callCount = 0;
        _cache.GetOrCreate("k5", () => { callCount++; return 1; }, TimeSpan.FromMinutes(5));
        _cache.Remove("k5");
        _cache.GetOrCreate("k5", () => { callCount++; return 2; }, TimeSpan.FromMinutes(5));

        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>RemoveByPrefixAsync</c> throws <see cref="NotSupportedException"/>
    /// because <see cref="IDistributedCache"/> has no key-scanning API.
    /// </summary>
    [Fact]
    public async Task RemoveByPrefixAsync_ThrowsNotSupportedException()
    {
        await Should.ThrowAsync<NotSupportedException>(
            () => _cache.RemoveByPrefixAsync("__IAnything__"));
    }

    /// <summary>
    /// Verifies that concurrent <c>GetOrCreateAsync</c> calls for the same key invoke
    /// the factory only once (per-key stampede protection via <see cref="System.Threading.SemaphoreSlim"/>).
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ConcurrentCalls_CallsFactoryOnce()
    {
        var factoryCalls = 0;
        var barrier = new TaskCompletionSource<bool>();

        Func<CancellationToken, Task<string>> factory = async _ =>
        {
            Interlocked.Increment(ref factoryCalls);
            await barrier.Task;
            return "concurrent-result";
        };

        var tasks = new Task<string>[5];
        for (var i = 0; i < 5; i++)
            tasks[i] = _cache.GetOrCreateAsync("k_concurrent", factory, TimeSpan.FromMinutes(5));

        barrier.SetResult(true);
        var results = await Task.WhenAll(tasks);

        factoryCalls.ShouldBe(1);
        foreach (var r in results)
            r.ShouldBe("concurrent-result");
    }
}
