using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Semaphore synchronization context (should be consumed within a method, not giving the structure away as a parameter, nor returning it to somewhere!)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public record struct SemaphoreSyncContext : IDisposable
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Semaphore
        /// </summary>
        public readonly SemaphoreSlim Semaphore;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        internal SemaphoreSyncContext(in SemaphoreSlim semaphore) => Semaphore = semaphore;

        /// <summary>
        /// Is disposed?
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Is synchronized?
        /// </summary>
        public readonly bool IsSynchronized => Semaphore.CurrentCount == 0;

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly void Sync(in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(timeout, cancellationToken);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly void Sync(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(cancellationToken);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly async Task SyncAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly async Task SyncAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Release the synchronization lock
        /// </summary>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly void Release()
        {
            lock (Semaphore)
                if (IsSynchronized)
                    Semaphore.Release();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            if (IsSynchronized)
                try
                {
                    Semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                }
        }

        /// <summary>
        /// Ensure undisposed state
        /// </summary>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly void EnsureUndisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        public static async Task<SemaphoreSyncContext> CreateAsync(SemaphoreSlim semaphore, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (timeout is null)
            {
                await semaphore.WaitAsync(cancellationToken).DynamicContext();
            }
            else
            {
                await semaphore.WaitAsync(timeout.Value, cancellationToken).DynamicContext();
            }
            return new(semaphore);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Task<SemaphoreSyncContext> CreateAsync(in SemaphoreSlim semaphore, in CancellationToken cancellationToken)
            => CreateAsync(semaphore, cancellationToken: cancellationToken);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        public static SemaphoreSyncContext Create(in SemaphoreSlim semaphore, in TimeSpan? timeout = null, in CancellationToken cancellationToken = default)
        {
            if (timeout is null)
            {
                semaphore.Wait(cancellationToken);
            }
            else
            {
                semaphore.Wait(timeout.Value, cancellationToken);
            }
            return new(semaphore);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static SemaphoreSyncContext Create(in SemaphoreSlim semaphore, in CancellationToken cancellationToken) => Create(semaphore, cancellationToken: cancellationToken);
    }
}
