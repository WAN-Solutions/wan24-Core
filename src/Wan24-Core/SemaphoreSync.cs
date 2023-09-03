using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Semaphore synchronization
    /// </summary>
    public sealed class SemaphoreSync : DisposableBase
    {
        /// <summary>
        /// Constuctor
        /// </summary>
        public SemaphoreSync() : base(asyncDisposing: false) { }

        /// <summary>
        /// Semaphore
        /// </summary>
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        /// <summary>
        /// Is synchronized?
        /// </summary>
        public bool IsSynchronized => Semaphore.CurrentCount == 0;

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public SemaphoreSyncContext SyncContext(in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(timeout, cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public SemaphoreSyncContext SyncContext(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<SemaphoreSyncContext> SyncContextAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext();
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<SemaphoreSyncContext> SyncContextAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Sync(TimeSpan timeout, CancellationToken cancellationToken = default) => IfUndisposed(() => Semaphore.Wait(timeout, cancellationToken));

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Sync(CancellationToken cancellationToken = default) => IfUndisposed(() => Semaphore.Wait(cancellationToken));

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public async Task SyncAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
            => await IfUndisposed(async () => await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext()).DynamicContext();

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public async Task SyncAsync(CancellationToken cancellationToken = default) => await IfUndisposed(async () => await Semaphore.WaitAsync(cancellationToken)).DynamicContext();

        /// <summary>
        /// Release the synchronization lock
        /// </summary>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Release()
        {
            EnsureUndisposed(allowDisposing: true);
            lock (Semaphore) if (IsSynchronized) Semaphore.Release();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public void Execute(in Action action, in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(timeout, cancellationToken);
            action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public void Execute(in Action action, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(cancellationToken);
            action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public T Execute<T>(in Func<T> action, in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(timeout, cancellationToken);
            return action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public T Execute<T>(in Func<T> action, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(cancellationToken);
            return action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task ExecuteAsync(Func<Task> action, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(timeout, cancellationToken).DynamicContext();
            await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(cancellationToken).DynamicContext();
            await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(timeout, cancellationToken).DynamicContext();
            return await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(cancellationToken).DynamicContext();
            return await action().DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Semaphore.Dispose();

        /// <summary>
        /// Cast as synchronization context
        /// </summary>
        /// <param name="sync">Synchronization</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SemaphoreSyncContext(in SemaphoreSync sync) => sync.SyncContext();
    }
}
