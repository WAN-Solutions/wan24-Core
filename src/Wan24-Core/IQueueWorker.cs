namespace wan24.Core
{
    /// <summary>
    /// Interface for a queue worker
    /// </summary>
    public interface IQueueWorker : IDisposable
    {
        /// <summary>
        /// Number of queued items
        /// </summary>
        int Queued { get; }
        /// <summary>
        /// Enqueue
        /// </summary>
        /// <param name="task">Task</param>
        ValueTask EnqueueAsync(QueueWorker.Task_Delegate task);
    }
}
