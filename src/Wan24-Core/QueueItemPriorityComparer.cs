namespace wan24.Core
{
    /// <summary>
    /// Queue item priority comparer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed class QueueItemPriorityComparer() : IComparer<IQueueItemPriority>
    {
        /// <inheritdoc/>
        public int Compare(IQueueItemPriority? x, IQueueItemPriority? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return x.CompareTo(y);
        }
    }
}
