namespace wan24.Core
{
    /// <summary>
    /// Rented thread pool
    /// </summary>
    public class RentedThreadPool : DisposableObjectPool<RentedThread>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="options">Options</param>
        public RentedThreadPool(in int capacity, in Func<RentedThread> factory, in RentedThreadOptions? options = null) : base(capacity, factory)
        {
            Options = options ?? new();
            ResetOnRent = false;
            ForceResetOnReturn = true;
        }

        /// <summary>
        /// Options
        /// </summary>
        public RentedThreadOptions Options { get; }

        /// <summary>
        /// Work
        /// </summary>
        /// <param name="worker">Worker</param>
        /// <param name="cancellationToken">Cancellation token (used for thread synchronization only)</param>
        public virtual async Task WorkAsync(RentedThread.Worker_Delegate worker, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            RentedObject<RentedThread> thread = new(this);
            await using (thread.DynamicContext())
                await thread.Object.WorkAsync(worker, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Work
        /// </summary>
        /// <param name="cancellationToken">Cancellation token (used for thread synchronization only)</param>
        /// <param name="workers">Workers (CAUTION: When overflowing the capacity of this pool, new threads will be created for the number of overflowing workers!)</param>
        public virtual async Task WorkManyAsync(CancellationToken cancellationToken, params RentedThread.Worker_Delegate[] workers)
        {
            EnsureUndisposed();
            int count = workers.Length;
            if (count < 1) return;
            await Task.Yield();
            using RentedArrayStructSimple<Task> tasks = new(len: count, clean: false);
            Task[] tasksArray = tasks.Array;
            for (int i = 0; i < count; tasksArray[i] = WorkAsync(workers[i], cancellationToken), i++) ;
            await Task.WhenAll(tasks.Array.Enumerate(offset: 0, count)).DynamicContext();
        }

        /// <summary>
        /// Create a rented thread pool
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="options">Options</param>
        /// <returns>Rented thread pool</returns>
        public static RentedThreadPool Create(in int capacity, RentedThreadOptions? options = null)
        {
            options ??= new();
            return new(capacity, () => new(options), options);
        }
    }
}
