using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="DefaultDecoratorCache"/> covering get/create, expiration,
/// concurrent stampede protection, removal, and prefix-based invalidation.
/// </summary>
public sealed class DefaultDecoratorCacheTests : IDisposable
{
    private readonly DefaultDecoratorCache _cache = new();

    /// <inheritdoc/>
    public void Dispose() => _cache.Dispose();

    /// <summary>
    /// Verifies that <c>GetOrCreateAsync</c> calls the factory exactly once for a new key
    /// and returns the value.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_NewKey_CallsFactoryAndReturnsValue()
    {
        var factoryCallCount = 0;
        var result = await _cache.GetOrCreateAsync<string>(
            "k1",
            async _ => { factoryCallCount++; return await Task.FromResult("hello"); },
            TimeSpan.FromMinutes(5));

        result.ShouldBe("hello");
        factoryCallCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that a second call for the same key returns the cached value without
    /// invoking the factory again.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ExistingKey_DoesNotCallFactoryAgain()
    {
        var factoryCallCount = 0;
        Func<CancellationToken, Task<string>> factory = async _ =>
        {
            factoryCallCount++;
            return await Task.FromResult("cached");
        };

        await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));
        var second = await _cache.GetOrCreateAsync("k2", factory, TimeSpan.FromMinutes(5));

        second.ShouldBe("cached");
        factoryCallCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that an expired entry is evicted and the factory is called again.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ExpiredEntry_CallsFactoryAgain()
    {
        var callCount = 0;
        Func<CancellationToken, Task<int>> factory = async _ =>
        {
            callCount++;
            return await Task.FromResult(callCount);
        };

        // Very short TTL so it expires immediately
        await _cache.GetOrCreateAsync("k3", factory, TimeSpan.FromMilliseconds(1));
        await Task.Delay(10); // let it expire

        var second = await _cache.GetOrCreateAsync("k3", factory, TimeSpan.FromMinutes(5));

        second.ShouldBe(2);
        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>GetOrCreate</c> (synchronous) calls the factory once and caches.
    /// </summary>
    [Fact]
    public void GetOrCreate_NewKey_CallsFactoryAndReturnsValue()
    {
        var callCount = 0;
        var result = _cache.GetOrCreate("k4", () => { callCount++; return 42; }, TimeSpan.FromMinutes(5));

        result.ShouldBe(42);
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that <c>GetOrCreate</c> returns the cached value on subsequent calls.
    /// </summary>
    [Fact]
    public void GetOrCreate_ExistingKey_DoesNotCallFactoryAgain()
    {
        var callCount = 0;
        _cache.GetOrCreate("k5", () => { callCount++; return "first"; }, TimeSpan.FromMinutes(5));
        var second = _cache.GetOrCreate("k5", () => { callCount++; return "second"; }, TimeSpan.FromMinutes(5));

        second.ShouldBe("first");
        callCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that <c>RemoveAsync</c> evicts the cached entry.
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

        await _cache.GetOrCreateAsync("k6", factory, TimeSpan.FromMinutes(5));
        await _cache.RemoveAsync("k6");

        var afterRemoval = await _cache.GetOrCreateAsync("k6", factory, TimeSpan.FromMinutes(5));

        afterRemoval.ShouldBe("value");
        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>Remove</c> (synchronous) evicts the cached entry.
    /// </summary>
    [Fact]
    public void Remove_ExistingKey_EvictsEntry()
    {
        var callCount = 0;
        _cache.GetOrCreate("k7", () => { callCount++; return "v"; }, TimeSpan.FromMinutes(5));
        _cache.Remove("k7");

        _cache.GetOrCreate("k7", () => { callCount++; return "v"; }, TimeSpan.FromMinutes(5));

        callCount.ShouldBe(2);
    }

    /// <summary>
    /// Verifies that <c>RemoveByPrefixAsync</c> evicts all keys starting with the given prefix.
    /// </summary>
    [Fact]
    public async Task RemoveByPrefixAsync_MatchingKeys_EvictsAll()
    {
        var callCount = 0;
        Func<CancellationToken, Task<int>> factory = async _ => { callCount++; return await Task.FromResult(callCount); };

        await _cache.GetOrCreateAsync("__ISvc__GetA", factory, TimeSpan.FromMinutes(5));
        await _cache.GetOrCreateAsync("__ISvc__GetB", factory, TimeSpan.FromMinutes(5));
        await _cache.GetOrCreateAsync("__IOther__Get", factory, TimeSpan.FromMinutes(5));

        await _cache.RemoveByPrefixAsync("__ISvc__");

        var a2 = await _cache.GetOrCreateAsync("__ISvc__GetA", factory, TimeSpan.FromMinutes(5));
        var other = await _cache.GetOrCreateAsync("__IOther__Get", factory, TimeSpan.FromMinutes(5));

        // __ISvc__ keys were evicted → factory was called again
        a2.ShouldBe(4); // calls 1+2 initial, 3 for factory call on __ISvc__GetA after eviction
        // __IOther__ was NOT evicted → returned from cache (call 3 from initial)
        other.ShouldBe(3);
    }

    /// <summary>
    /// Verifies that concurrent <c>GetOrCreateAsync</c> calls for the same key
    /// invoke the factory only once (stampede protection).
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
            return "result";
        };

        // Fire 5 concurrent calls for the same key
        var tasks = new List<Task<string>>();
        for (var i = 0; i < 5; i++)
            tasks.Add(_cache.GetOrCreateAsync("k_concurrent", factory, TimeSpan.FromMinutes(5)));

        barrier.SetResult(true);
        var results = await Task.WhenAll(tasks);

        factoryCalls.ShouldBe(1);
        foreach (var r in results)
            r.ShouldBe("result");
    }
}
