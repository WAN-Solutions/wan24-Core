namespace wan24.Core
{
    /// <summary>
    /// Base class for a parallel item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [Obsolete("Use ParallelItemQueueWorkerBase instead")]
    public abstract class ParallelItemQueueWorker<T> : ParallelItemQueueWorkerBase<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="threads">Number of threads</param>
        [Obsolete("Use ParallelItemQueueWorkerBase instead")]
        protected ParallelItemQueueWorker(in int capacity, in int threads) : base(capacity, threads) { }
    }
}
