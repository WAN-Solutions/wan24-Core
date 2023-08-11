using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Semaphore synchronization
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public record struct SemaphoreSync : IDisposable
    {
        /// <summary>
        /// Semaphore
        /// </summary>
        public readonly SemaphoreSlim Semaphore = new(1, 1);

        /// <summary>
        /// Constuctor
        /// </summary>
        public SemaphoreSync() { }

        /// <summary>
        /// Is disposed?
        /// </summary>
        public bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        public readonly SemaphoreSyncContext Sync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            Semaphore.Wait(timeout, cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        public readonly SemaphoreSyncContext Sync(CancellationToken cancellationToken = default)
        {
            Semaphore.Wait(cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        public readonly async Task<SemaphoreSyncContext> SyncAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext();
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        public readonly async Task<SemaphoreSyncContext> SyncAsync(CancellationToken cancellationToken = default)
        {
            await Semaphore.WaitAsync(cancellationToken);
            return new(Semaphore);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (Semaphore)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            Semaphore.Dispose();
        }

        /// <summary>
        /// Cast as synchronization context
        /// </summary>
        /// <param name="sync">Synchronization</param>
        public static implicit operator SemaphoreSyncContext(SemaphoreSync sync) => sync.Sync();
    }
}
