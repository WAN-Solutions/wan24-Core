namespace wan24.Core
{
    /// <summary>
    /// Base class for a <see cref="PriorityQueueWorker{T}"/> priority (to be compared using <see cref="QueueItemPriorityComparer"/>)
    /// </summary>
    public abstract class QueueItemPriorityBase : IQueueItemPriority
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected QueueItemPriorityBase() { }

        /// <inheritdoc/>
        public abstract int CompareTo(IQueueItemPriority? other);
    }
}
