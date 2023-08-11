using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Semaphore synchronization context
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public record struct SemaphoreSyncContext : IDisposable
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Semaphore
        /// </summary>
        private readonly SemaphoreSlim Semaphore;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        internal SemaphoreSyncContext(SemaphoreSlim semaphore) => Semaphore = semaphore;

        /// <summary>
        /// Is disposed?
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            Semaphore.Release();
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
        public static Task<SemaphoreSyncContext> CreateAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
            => CreateAsync(semaphore, cancellationToken: cancellationToken);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        public static SemaphoreSyncContext Create(SemaphoreSlim semaphore, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
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
        public static SemaphoreSyncContext Create(SemaphoreSlim semaphore, CancellationToken cancellationToken) => Create(semaphore, cancellationToken: cancellationToken);
    }
}
