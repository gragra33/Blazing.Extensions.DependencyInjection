using System.Diagnostics.CodeAnalysis;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// An async-compatible mutual exclusion lock backed by <see cref="SemaphoreSlim"/>.
/// Suitable for use inside <c>using</c> blocks that contain <c>await</c> expressions —
/// unlike the <c>lock</c> keyword, which prohibits <c>await</c>.
/// </summary>
/// <remarks>
/// <para>
/// The handle returned by <see cref="LockAsync"/> is pre-allocated in the constructor
/// and reused on every call, meaning <see cref="LockAsync"/> produces zero heap allocations
/// on the happy path.
/// </para>
/// <para>
/// If the lock has been disposed or the provided <see cref="CancellationToken"/> is cancelled
/// before acquisition, a singleton no-op handle is returned instead of throwing; the
/// caller's <c>using</c> block remains safe.
/// </para>
/// </remarks>
public sealed class AsyncLock : IDisposable
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed",
        Justification = "_semaphore is disposed via Interlocked.Exchange for atomic once-only disposal; the analyzer does not recognise this indirect pattern.")]
    private SemaphoreSlim? _semaphore = new(1, 1);
    private readonly LockHandle _handle;

    /// <summary>
    /// 1 when the semaphore is currently held by a caller; 0 when free.
    /// Guards against double-release when the same pre-allocated handle is referenced
    /// by multiple <c>using</c> variables.
    /// </summary>
    private int _held;

    /// <summary>Initializes a new instance of <see cref="AsyncLock"/>.</summary>
    public AsyncLock() => _handle = new LockHandle(this);

    /// <summary>
    /// Asynchronously acquires the lock and returns a handle that frees it when disposed.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token observed during acquisition. If cancelled, a no-op handle is returned.
    /// </param>
    /// <returns>
    /// A task that completes with an <see cref="IDisposable"/> whose <c>Dispose</c>
    /// releases the lock.
    /// </returns>
    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        var semaphore = _semaphore;
        if (semaphore is null)
            return NullHandle.Instance;

        try
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            Interlocked.Exchange(ref _held, 1);
            return _handle;
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
        {
            return NullHandle.Instance;
        }
    }

    /// <summary>
    /// Releases the semaphore exactly once per acquisition.
    /// Called by <see cref="LockHandle.Dispose"/>; idempotent across multiple calls.
    /// </summary>
    private void Unlock()
    {
        // Only release if this instance currently holds the semaphore.
        // Prevents double-release when the pre-allocated handle is referenced
        // by more than one using variable.
        if (Interlocked.CompareExchange(ref _held, 0, 1) != 1)
            return;

        try { _semaphore?.Release(); }
        catch (ObjectDisposedException)
        {
            // Semaphore was disposed concurrently — no action required.
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Interlocked.Exchange(ref _semaphore, null)?.Dispose();
        _handle.Dispose(); // Unlock() null-checks _semaphore — safe after disposal
    }

    private sealed class LockHandle(AsyncLock owner) : IDisposable
    {
        public void Dispose() => owner.Unlock();
    }

    private sealed class NullHandle : IDisposable
    {
        /// <summary>Gets the singleton no-op handle instance.</summary>
        public static readonly NullHandle Instance = new();

        private NullHandle() { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
