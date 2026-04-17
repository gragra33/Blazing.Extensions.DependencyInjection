using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="AsyncLock"/> — covering serial acquisition, concurrent
/// single-flight semantics, cancellation, disposal, and null-handle safety.
/// </summary>
public sealed class AsyncLockTests
{
    /// <summary>
    /// Verifies that a single <see cref="AsyncLock.LockAsync"/> acquires the lock and
    /// that disposing the returned handle releases it so a subsequent call can acquire.
    /// </summary>
    [Fact]
    public async Task LockAsync_AcquiresAndReleases()
    {
        using var asyncLock = new AsyncLock();

        var handle = await asyncLock.LockAsync();
        handle.ShouldNotBeNull();
        handle.Dispose();

        // Should be able to acquire again after release
        var handle2 = await asyncLock.LockAsync();
        handle2.ShouldNotBeNull();
        handle2.Dispose();
    }

    /// <summary>
    /// Verifies that two sequential calls both succeed and return real (non-null) handles.
    /// </summary>
    [Fact]
    public async Task LockAsync_SerialCalls_BothSucceed()
    {
        using var asyncLock = new AsyncLock();

        using var first = await asyncLock.LockAsync();
        first.ShouldNotBeNull();
        // Dispose releases before second acquire
        first.Dispose();

        using var second = await asyncLock.LockAsync();
        second.ShouldNotBeNull();
    }

    /// <summary>
    /// Verifies single-flight semantics: 10 concurrent callers all eventually acquire the lock,
    /// but only one at a time can hold it — the counter increments correctly with no races.
    /// </summary>
    [Fact]
    public async Task LockAsync_ConcurrentCalls_SingleFlight()
    {
        using var asyncLock = new AsyncLock();
        var callCount = 0;
        var maxConcurrent = 0;
        var currentConcurrent = 0;

        async Task WorkAsync()
        {
            using (await asyncLock.LockAsync())
            {
                var concurrent = Interlocked.Increment(ref currentConcurrent);
                if (concurrent > maxConcurrent)
                    Interlocked.Exchange(ref maxConcurrent, concurrent);

                // Simulate work
                await Task.Yield();

                Interlocked.Increment(ref callCount);
                Interlocked.Decrement(ref currentConcurrent);
            }
        }

        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
            tasks.Add(Task.Run(WorkAsync));

        await Task.WhenAll(tasks);

        callCount.ShouldBe(10);
        maxConcurrent.ShouldBe(1, "At most one caller should hold the lock at a time");
    }

    /// <summary>
    /// Verifies that <see cref="AsyncLock.LockAsync"/> after <see cref="AsyncLock.Dispose"/>
    /// returns a no-op handle rather than throwing.
    /// </summary>
    [Fact]
    public async Task LockAsync_AfterDispose_ReturnsNullHandle()
    {
        var asyncLock = new AsyncLock();
        asyncLock.Dispose();

        var handle = await asyncLock.LockAsync();

        handle.ShouldNotBeNull();
        // Must not throw
        await Should.NotThrowAsync(async () => handle.Dispose());
    }

    /// <summary>
    /// Verifies that a pre-cancelled <see cref="CancellationToken"/> causes
    /// <see cref="AsyncLock.LockAsync"/> to return a no-op handle rather than throwing.
    /// </summary>
    [Fact]
    public async Task LockAsync_CancelledToken_ReturnsNullHandle()
    {
        using var asyncLock = new AsyncLock();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var handle = await asyncLock.LockAsync(cts.Token);

        handle.ShouldNotBeNull();
        await Should.NotThrowAsync(async () => handle.Dispose());
    }

    /// <summary>
    /// Verifies that cancelling while waiting to acquire also returns a no-op handle.
    /// </summary>
    [Fact]
    public async Task LockAsync_CancelWhileWaiting_ReturnsNullHandle()
    {
        using var asyncLock = new AsyncLock();

        // Hold the lock so the second caller must wait
        var holder = await asyncLock.LockAsync();

        using var cts = new CancellationTokenSource();
        var waitTask = asyncLock.LockAsync(cts.Token);

        // Cancel while waitTask is queued
        await cts.CancelAsync();

        var handle = await waitTask;

        handle.ShouldNotBeNull();
        await Should.NotThrowAsync(async () => handle.Dispose());

        // Release the original holder
        holder.Dispose();
    }

    /// <summary>
    /// Verifies that calling <see cref="IDisposable.Dispose"/> on the null handle
    /// multiple times never throws.
    /// </summary>
    [Fact]
    public async Task NullHandle_Dispose_IsNoOp()
    {
        var asyncLock = new AsyncLock();
        asyncLock.Dispose();

        var handle = await asyncLock.LockAsync();

        await Should.NotThrowAsync(async () => handle.Dispose());
        await Should.NotThrowAsync(async () => handle.Dispose());
        await Should.NotThrowAsync(async () => handle.Dispose());
    }

    /// <summary>
    /// Verifies that calling <see cref="AsyncLock.Dispose"/> twice is idempotent.
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var firstCallCompleted = false;
        using (var asyncLock = new AsyncLock())
        {
            asyncLock.Dispose();       // First explicit call
            firstCallCompleted = true; // Reached only if first call did not throw
        }                              // using block calls Dispose() a second time

        firstCallCompleted.ShouldBeTrue("First Dispose() must not throw");
        // If the using block's implicit second Dispose() threw, the test would already have failed
    }

    /// <summary>
    /// Verifies that disposing the real lock handle twice does not throw.
    /// </summary>
    [Fact]
    public async Task LockAsync_HandleDisposedTwice_IsIdempotent()
    {
        using var asyncLock = new AsyncLock();

        var handle = await asyncLock.LockAsync();

        await Should.NotThrowAsync(async () => handle.Dispose());
        await Should.NotThrowAsync(async () => handle.Dispose());
    }
}
