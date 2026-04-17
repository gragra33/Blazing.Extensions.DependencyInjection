using System.Diagnostics.CodeAnalysis;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// An async-compatible mutual exclusion lock backed by <see cref="SemaphoreSlim"/>.
/// Suitable for use inside <c>using</c> blocks that contain <c>await</c> expressions —
/// unlike the <c>lock</c> keyword, which prohibits <c>await</c>.
/// </summary>
/// <remarks>
/// <para>
/// Each call to <see cref="LockAsync"/> returns a new <see cref="IDisposable"/> handle tied
/// to that specific acquisition. Disposing the handle releases the lock exactly once;
/// subsequent calls to <see cref="IDisposable.Dispose"/> on the same handle are no-ops.
/// This prevents a late or double-disposed handle from inadvertently releasing a lock
/// held by a different, concurrent acquisition.
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

    /// <summary>Initializes a new instance of <see cref="AsyncLock"/>.</summary>
    public AsyncLock() { }

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
            return new LockHandle(this);
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
        {
            return NullHandle.Instance;
        }
    }

    /// <summary>
    /// Releases the semaphore. Called by a <see cref="LockHandle"/> exactly once
    /// (the handle's own release flag prevents double-release).
    /// </summary>
    private void Unlock()
    {
        try { _semaphore?.Release(); }
        catch (ObjectDisposedException)
        {
            // Semaphore was disposed concurrently — no action required.
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => Interlocked.Exchange(ref _semaphore, null)?.Dispose();

    /// <summary>
    /// Per-acquisition handle. Each call to <see cref="LockAsync"/> that succeeds
    /// creates a new instance; the <see cref="_released"/> flag ensures <see cref="Dispose"/>
    /// is idempotent and that disposing a stale handle cannot release a different
    /// caller's acquisition.
    /// </summary>
    private sealed class LockHandle(AsyncLock owner) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                owner.Unlock();
        }
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
